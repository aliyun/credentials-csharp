namespace Aliyun.Credentials.Policy
{
    public enum StaleValueBehavior
    {
        /// <summary>
        /// Strictly treat the stale time. Never return a stale cached value (except when the supplier returns an expired value,
        /// in which case the supplier will return the value but only for a very short period of time to prevent overloading
        /// the underlying supplier).
        /// </summary>
        Strict,

        /// <summary>
        /// Allow stale values to be returned from the cache. Value retrieval will never fail, as long as the cache has
        /// succeeded when calling the underlying supplier at least once.
        /// </summary>
        Allow
    }
}

