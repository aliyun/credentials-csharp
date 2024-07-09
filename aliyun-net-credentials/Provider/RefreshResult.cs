
namespace Aliyun.Credentials.Provider
{
    public sealed class RefreshResult<T>
    {
        private readonly T _value;

        private readonly long _staleTime;

        public T Value
        {
            get { return _value; }
        }

        public long StaleTime
        {
            get { return _staleTime; }
        }

        public RefreshResult(T value, long staleTime)
        {
            _value = value;
            _staleTime = staleTime;
        }
    }
}
