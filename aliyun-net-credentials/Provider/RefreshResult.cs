namespace Aliyun.Credentials.Provider
{
    public sealed class RefreshResult<T>
    {
        private readonly T value;
        private long staleTime;
        private long prefetchTime = long.MaxValue;

        public T Value
        {
            get { return value; }
        }

        public long StaleTime
        {
            get { return staleTime; }
        }

        public long PrefetchTime
        {
            get { return prefetchTime; }
        }

        public RefreshResult(T value, long staleTime)
        {
            this.value = value;
            this.staleTime = staleTime;
        }

        private RefreshResult(Builder builder)
        {
            this.value = builder.value;
            this.staleTime = builder.staleTime;
            this.prefetchTime = builder.prefetchTime;
        }
        
        public Builder ToBuilder()
        {
            return new Builder(this);
        }

        public override string ToString()
        {
            return string.Format("RefreshResult(value={0}, staleTime={1}, prefetchTime={2})", value, staleTime,
                prefetchTime);
        }

        public class Builder
        {
            internal T value;
            internal long staleTime = long.MaxValue;
            internal long prefetchTime = long.MaxValue;

            public Builder(RefreshResult<T> refreshResult)
            {
                this.value = refreshResult.Value;
                this.staleTime = refreshResult.StaleTime;
                this.prefetchTime = refreshResult.PrefetchTime;
            }
            
            internal Builder(T value)
            {
                this.value = value;
            }

            public Builder StaleTime(long staleTime)
            {
                this.staleTime = staleTime;
                return this;
            }
            
            public Builder PrefetchTime(long prefetchTime)
            {
                this.prefetchTime = prefetchTime;
                return this;
            }
            
            public RefreshResult<T> Build()
            {
                return new RefreshResult<T>(this);
            }
        }
    }
}