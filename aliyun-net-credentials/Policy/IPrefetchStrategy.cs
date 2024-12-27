using System;
using System.Threading.Tasks;

namespace Aliyun.Credentials.Policy
{
    public interface IPrefetchStrategy : IDisposable
    {
        void Prefetch(Action valueUpdater);
        Task  PrefetchAsync(Func<Task> valueUpdater);
    }
}
