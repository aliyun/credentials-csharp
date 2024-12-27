using System;
using System.Threading;
using System.Threading.Tasks;
using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Logging;
using Aliyun.Credentials.Policy;
using Aliyun.Credentials.Utils;

namespace Aliyun.Credentials.Provider
{
    public class RefreshCachedSupplier<T>
    {
        private static readonly ILog Logger = LogProvider.For<RefreshCachedSupplier<T>>();
        private const long StaleTime = 15 * 60 * 1000;

        /// <summary>
        /// Maximum time to wait for a blocking refresh lock before calling refresh again. Unit of milliseconds.
        /// </summary>
        private const int RefreshBlockingMaxWait = 5 * 1000;

        private readonly object refreshLock = new object();
        private readonly SemaphoreSlim refreshLockAsync = new SemaphoreSlim(1, 1);
        private volatile RefreshResult<T> cachedValue;
        private readonly Func<RefreshResult<T>> refreshFunc;
        private readonly Func<Task<RefreshResult<T>>> refreshFuncAsync;

        private volatile int consecutiveRefreshFailures;
        private readonly StaleValueBehavior staleValueBehavior;

        private readonly Random jitter = new Random();
        private readonly IPrefetchStrategy prefetchStrategy;

        public RefreshCachedSupplier(Func<RefreshResult<T>> refreshFunc,
            Func<Task<RefreshResult<T>>> refreshFuncAsync)
        {
            this.refreshFunc =
                ParameterHelper.ValidateNotNull(refreshFunc, "refreshFunc", "RefreshFunc must not be null.");
            this.refreshFuncAsync = ParameterHelper.ValidateNotNull(refreshFuncAsync, "refreshFuncAsync",
                "RefreshFuncAsync must not be null.");
            this.staleValueBehavior = StaleValueBehavior.Strict;
            this.prefetchStrategy = new OneCallerBlocks();
        }

        private RefreshCachedSupplier(Builder builder)
        {
            this.staleValueBehavior = ParameterHelper.ValidateNotNull(builder.staleValueBehavior,
                "this.staleValueBehavior", "StaleValueBehavior is null.");
            ParameterHelper.ValidateNotNull(builder.jitterEnabled, "jitterEnabled", "JitterEnabled is null.");
            this.refreshFunc =
                ParameterHelper.ValidateNotNull(builder.refreshFunc, "refreshFunc", "Refresh Function is null.");
            this.refreshFuncAsync =
                ParameterHelper.ValidateNotNull(builder.refreshFuncAsync, "refreshFuncAsync",
                    "RefreshAsync Function is null.");
            this.prefetchStrategy = builder.asyncUpdateEnabled
                ? (IPrefetchStrategy)new NonBlocking()
                : new OneCallerBlocks();
        }

        public T Get()
        {
            if (CacheIsStale())
            {
                Logger.Debug("Refreshing credentials synchronously");
                RefreshCache();
            }
            else if (ShouldInitiateCachePrefetch())
            {
                Logger.Debug("Prefetching credentials, using prefetch strategy: {0}",
                    this.prefetchStrategy.ToString());
                PrefetchCache();
            }

            return this.cachedValue.Value;
        }

        public async Task<T> GetAsync()
        {
            if (CacheIsStale())
            {
                Logger.Debug("Refreshing credentials synchronously");
                await RefreshCacheAsync();
            }
            else if (ShouldInitiateCachePrefetch())
            {
                Logger.Debug("Prefetching credentials, using prefetch strategy: {0}",
                    this.prefetchStrategy.ToString());
                await PrefetchCacheAsync();
            }

            return this.cachedValue.Value;
        }

        private void PrefetchCache()
        {
            this.prefetchStrategy.Prefetch(RefreshCache);
        }
        
        private async Task PrefetchCacheAsync()
        {
            await this.prefetchStrategy.PrefetchAsync(RefreshCacheAsync);
        }

        private void RefreshCache()
        {
            var lockTaken = false;
            try
            {
                lockTaken = Monitor.TryEnter(this.refreshLock, RefreshBlockingMaxWait);

                if (lockTaken && (CacheIsStale() || ShouldInitiateCachePrefetch()))
                {
                    try
                    {
                        this.cachedValue = HandleFetchedSuccess(this.refreshFunc());
                    }
                    catch (Exception ex)
                    {
                        this.cachedValue = HandleFetchedFailure(ex);
                    }
                }
            }
            catch (ThreadInterruptedException)
            {
                Thread.CurrentThread.Interrupt();
                throw new InvalidOperationException("Interrupted waiting to refresh the value.");
            }
            catch (CredentialException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CredentialException("Failed to refresh credentials.", ex.Message);
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(this.refreshLock);
                }
            }
        }

        private RefreshResult<T> HandleFetchedSuccess(RefreshResult<T> value)
        {
            Logger.Debug(string.Format("Refresh credentials successfully, retrieved value is {0}, cached value is {1}",
                value, this.cachedValue));
            Interlocked.Exchange(ref consecutiveRefreshFailures, 0);
            var now = DateTime.UtcNow.GetTimeMillis();
            // 过期时间大于15分钟，不用管
            if (now < value.StaleTime)
            {
                Logger.Debug(string.Format("Retrieved value stale time is {0}. Using staleTime of {1}",
                    ParameterHelper.FormatIso8601Date(value.StaleTime),
                    ParameterHelper.FormatIso8601Date(value.StaleTime)));
                return value;
            }

            // 不足或等于15分钟，但未过期，下次会再次刷新
            if (now < value.StaleTime + StaleTime)
            {
                Logger.Warn(string.Format("Retrieved value stale time is %s in the past ({0}). Using staleTime of {1}",
                    ParameterHelper.FormatIso8601Date(value.StaleTime),
                    ParameterHelper.FormatIso8601Date(now)));
                return value.ToBuilder().StaleTime(now).Build();
            }

            Logger.Warn(string.Format(
                "Retrieved value expiration time of the credential is in the past ({0}). Trying use the cached value.",
                ParameterHelper.FormatIso8601Date(value.StaleTime + StaleTime)));

            // 已过期，看缓存，缓存若大于15分钟，返回缓存，若小于15分钟，则根据策略判断是立刻重试还是稍后重试
            if (this.cachedValue == null)
            {
                throw new CredentialException("No cached value was found.");
            }
            if (now < this.cachedValue.StaleTime)
            {
                Logger.Warn(string.Format("Cached value staleTime is {0}. Using staleTime of {1}",
                    ParameterHelper.FormatIso8601Date(this.cachedValue.StaleTime),
                    ParameterHelper.FormatIso8601Date(this.cachedValue.StaleTime)));
                return this.cachedValue;
            }

            switch (this.staleValueBehavior)
            {
                case StaleValueBehavior.Strict:
                    // 立马重试
                    Logger.Warn(string.Format(
                        "Cached value expiration is in the past ({0}). Using expiration of {1}",
                        value.StaleTime, now + 1000));
                    return this.cachedValue.ToBuilder().StaleTime(now + 1000).Build();
                case StaleValueBehavior.Allow:
                    //一分钟左右重试一次
                    var waitUntilNextRefresh = 50 * 1000 + jitter.Next(20 * 1000 + 1);
                    var nextRefreshTime = now + waitUntilNextRefresh;
                    Logger.Warn(string.Format(
                        "Cached value expiration has been extended to {0} because the downstream service returned a time in the past: {1}",
                        nextRefreshTime, value.StaleTime));
                    return this.cachedValue.ToBuilder().StaleTime(nextRefreshTime).Build();
                default:
                    throw new ArgumentException(string.Format("Unknown stale-value-behavior: {0}",
                        this.staleValueBehavior));
            }
        }

        private RefreshResult<T> HandleFetchedFailure(Exception exception)
        {
            Logger.Warn(string.Format("Refresh credentials failed, cached value is {0}, exception is {1}", this.cachedValue, exception));
            var currentCachedValue = this.cachedValue;
            if (currentCachedValue == null)
            {
                Logger.Error(exception.Message);
                throw exception;
            }

            var now = DateTime.UtcNow.GetTimeMillis();
            if (now < currentCachedValue.StaleTime)
            {
                return currentCachedValue;
            }

            var numFailures = Interlocked.Increment(ref consecutiveRefreshFailures);
            switch (this.staleValueBehavior)
            {
                case StaleValueBehavior.Strict:
                    throw exception;
                case StaleValueBehavior.Allow:
                    // 采用退避算法，立刻重试
                    var newStaleTime = JitterTime(now, 1000, MaxStaleFailureJitter(numFailures));
                    Logger.Warn(string.Format(
                        "Cached value expiration has been extended to {0} because calling the downstream service failed (consecutive failures: {1}).",
                        newStaleTime, numFailures));
                    return currentCachedValue.ToBuilder().StaleTime(newStaleTime).Build();
                default:
                    throw new ArgumentException(string.Format("Unknown stale-value-behavior: {0}",
                        this.staleValueBehavior));
            }
        }

        private long JitterTime(long time, long jitterStart, long jitterEnd)
        {
            var jitterRange = jitterEnd - jitterStart;
            var jitterAmount = Math.Abs(jitter.Next(0, (int)jitterRange));
            return time + jitterStart + jitterAmount;
        }

        private static long MaxStaleFailureJitter(int numFailures)
        {
            var exponentialBackoffMillis = (1L << (numFailures - 1)) * 100;
            return exponentialBackoffMillis > 10 * 1000 ? exponentialBackoffMillis : 10 * 1000;
        }

        private async Task RefreshCacheAsync()
        {
            var lockTaken = false;
            try
            {
                lockTaken = await refreshLockAsync.WaitAsync(RefreshBlockingMaxWait);

                if (lockTaken && (CacheIsStale() || ShouldInitiateCachePrefetch()))
                {
                    try
                    {
                        this.cachedValue = HandleFetchedSuccess(await refreshFuncAsync());
                    }
                    catch (Exception ex)
                    {
                        this.cachedValue = HandleFetchedFailure(ex);
                    }
                }
            }
            catch (ThreadInterruptedException)
            {
                Thread.CurrentThread.Interrupt();
                throw new InvalidOperationException("Interrupted waiting to refresh the value.");
            }
            catch (CredentialException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CredentialException("Failed to refresh credentials.", ex.Message);
            }
            finally
            {
                if (lockTaken)
                {
                    refreshLockAsync.Release();
                }
            }
        }

        private bool CacheIsStale()
        {
            return this.cachedValue == null || DateTime.UtcNow.GetTimeMillis() >= this.cachedValue.StaleTime;
        }

        private bool ShouldInitiateCachePrefetch()
        {
            return this.cachedValue == null || DateTime.UtcNow.GetTimeMillis() >= this.cachedValue.PrefetchTime;
        }

        public class Builder
        {
            internal readonly Func<RefreshResult<T>> refreshFunc;
            internal readonly Func<Task<RefreshResult<T>>> refreshFuncAsync;
            internal bool asyncUpdateEnabled;
            internal bool jitterEnabled = true;
            internal StaleValueBehavior staleValueBehavior = Policy.StaleValueBehavior.Strict;

            internal Builder(Func<RefreshResult<T>> refreshFunc, Func<Task<RefreshResult<T>>> refreshFuncAsync)
            {
                this.refreshFunc = refreshFunc;
                this.refreshFuncAsync = refreshFuncAsync;
            }

            public Builder AsyncUpdateEnabled(bool buildAsyncUpdateEnabled)
            {
                this.asyncUpdateEnabled = buildAsyncUpdateEnabled;
                return this;
            }

            public Builder StaleValueBehavior(StaleValueBehavior buildStaleValueBehavior)
            {
                this.staleValueBehavior = buildStaleValueBehavior;
                return this;
            }

            internal Builder JitterEnabled(bool jitterEnabledInBuilder)
            {
                this.jitterEnabled = jitterEnabledInBuilder;
                return this;
            }

            public RefreshCachedSupplier<T> Build()
            {
                return new RefreshCachedSupplier<T>(this);
            }
        }
    }
}