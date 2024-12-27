using System;
using System.Threading;
using System.Threading.Tasks;

namespace Aliyun.Credentials.Policy
{
    public class OneCallerBlocks : IPrefetchStrategy
    {
        private int currentlyRefreshing;

        public void Prefetch(Action valueUpdater)
        {
            if (Interlocked.CompareExchange(ref currentlyRefreshing, 1, 0) != 0) return;

            try
            {
                valueUpdater();
            }
            finally
            {
                Interlocked.Exchange(ref currentlyRefreshing, 0);
            }
        }

        public async Task PrefetchAsync(Func<Task> valueUpdater)
        {
            if (Interlocked.CompareExchange(ref currentlyRefreshing, 1, 0) != 0) return;

            try
            {
                await valueUpdater.Invoke();
            }
            finally
            {
                Interlocked.Exchange(ref currentlyRefreshing, 0);
            }
        }

        public void Dispose()
        {
        }
    }
}