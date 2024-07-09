using System;
using System.Threading;
using System.Threading.Tasks;
using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Utils;

namespace Aliyun.Credentials.Provider
{
    public class RefreshCachedSupplier<T>
    {
        private static readonly int REFRESH_BLOCKING_MAX_WAIT = 5 * 1000;
        private readonly object refreshLock = new object();
        private static readonly SemaphoreSlim refreshLockAsync = new SemaphoreSlim(1, 1);
        private volatile RefreshResult<T> cachedValue = new RefreshResult<T>(default(T), 0);
        private readonly Func<RefreshResult<T>> refreshFunc;
        private readonly Func<Task<RefreshResult<T>>> refreshFuncAsync;

        public RefreshCachedSupplier(Func<RefreshResult<T>> _refreshFunc, Func<Task<RefreshResult<T>>> _refreshFuncAsync)
        {
            refreshFunc = ParameterHelper.ValidateNotNull(_refreshFunc, "_refreshFunc", "RefreshFunc must not be null.");
            refreshFuncAsync = ParameterHelper.ValidateNotNull(_refreshFuncAsync, "_refreshFuncAsync", "RefreshFuncAsync must not be null.");
        }

        public T Get()
        {
            if (CacheIsStale())
            {
                Refresh();
            }
            return cachedValue != null ? cachedValue.Value : default(T);
        }

        public async Task<T> GetAsync()
        {
            if (CacheIsStale())
            {
                await RefreshAsync();
            }
            return cachedValue != null ? cachedValue.Value : default(T);
        }

        private void RefreshCache()
        {
            try
            {
                cachedValue = refreshFunc();
            }
            catch (CredentialException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new CredentialException("Failed to refresh credentials.");
            }
        }

        private async Task RefreshCacheAsync()
        {
            try
            {
                cachedValue = await refreshFuncAsync();
            }
            catch (CredentialException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new CredentialException("Failed to refresh credentials.");
            }
        }

        private void Refresh()
        {
            bool lockTaken = false;
            try
            {
                lockTaken = Monitor.TryEnter(refreshLock, REFRESH_BLOCKING_MAX_WAIT);

                if (lockTaken && CacheIsStale())
                {
                    RefreshCache();
                }
            }
            catch (ThreadInterruptedException)
            {
                Thread.CurrentThread.Interrupt();
                throw new InvalidOperationException("Interrupted waiting to refresh the value.");
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(refreshLock);
                }
            }
        }

        public async Task RefreshAsync()
        {
            bool lockTaken = false;
            try
            {
                lockTaken = await refreshLockAsync.WaitAsync(REFRESH_BLOCKING_MAX_WAIT);

                if (lockTaken)
                {
                    if (CacheIsStale())
                    {
                        await RefreshCacheAsync();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (lockTaken)
                {
                    refreshLockAsync.Release();
                }
            }
        }

        public bool CacheIsStale()
        {
            return cachedValue == null || SessionCredentialsProvider.GetUnixTimeMilliseconds(DateTimeOffset.UtcNow) >= this.cachedValue.StaleTime;
        }
    }
}