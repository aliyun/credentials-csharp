using System;
using System.Threading;
using System.Threading.Tasks;
using Aliyun.Credentials.Logging;

namespace Aliyun.Credentials.Policy
{
    public class NonBlocking : IPrefetchStrategy
    {
        private static readonly ILog Logger = LogProvider.For<NonBlocking>();

        private const int MaxConcurrentRefreshes = 100;

        private readonly SemaphoreSlim concurrentRefreshLeases =
            new SemaphoreSlim(MaxConcurrentRefreshes, MaxConcurrentRefreshes);

        // 0: false，1: true, default: 0
        private int currentlyRefreshing;

        public void Prefetch(Action valueUpdater)
        {
            if (Interlocked.CompareExchange(ref currentlyRefreshing, 1, 0) != 0)
            {
                return;
            }

            // 判断是否存在可用的资源
            if (!concurrentRefreshLeases.Wait(0))
            {
                Logger.Warn("Skipping a background refresh task because there are too many other tasks running.");
                // 将状态重置为 false
                Interlocked.Exchange(ref currentlyRefreshing, 0);
                return;
            }

            try
            {
                Task.Run(() =>
                {
                    try
                    {
                        valueUpdater.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex.Message);
                        throw;
                    }
                    finally
                    {
                        concurrentRefreshLeases.Release();
                        Interlocked.Exchange(ref currentlyRefreshing, 0);
                    }
                });
            }
            catch (Exception ex)
            {
                concurrentRefreshLeases.Release();
                Interlocked.Exchange(ref currentlyRefreshing, 0);
                Logger.Warn(ex.Message);
                throw;
            }
        }

        public async Task PrefetchAsync(Func<Task> valueUpdater)
        {
            if (Interlocked.CompareExchange(ref currentlyRefreshing, 1, 0) != 0)
            {
                return;
            }

            // 判断是否存在可用的资源
            if (!await concurrentRefreshLeases.WaitAsync(0))
            {
                Logger.Warn("Skipping a background refresh task because there are too many other tasks running.");
                // 将状态重置为 false
                Interlocked.Exchange(ref currentlyRefreshing, 0);
                return;
            }
            try
            {
                Task.Run(async () =>
                {
                    try
                    {
                        await valueUpdater.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex.Message);
                        throw;
                    }
                    finally
                    {
                        concurrentRefreshLeases.Release();
                        Interlocked.Exchange(ref currentlyRefreshing, 0);
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Warn(ex.Message);
                concurrentRefreshLeases.Release();
                Interlocked.Exchange(ref currentlyRefreshing, 0);
                throw;
            }
        }

        public void Dispose()
        {
            concurrentRefreshLeases.Dispose();
        }
    }
}