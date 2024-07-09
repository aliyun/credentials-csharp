using System;
using System.Threading.Tasks;
using Aliyun.Credentials.Models;

namespace Aliyun.Credentials.Provider
{
    public abstract class SessionCredentialsProvider : IAlibabaCloudCredentialsProvider
    {

        private RefreshCachedSupplier<CredentialModel> credentialsCache;
        private readonly Func<RefreshResult<CredentialModel>> refreshFunc;
        private readonly Func<Task<RefreshResult<CredentialModel>>> refreshFuncAsync;

        public abstract RefreshResult<CredentialModel> RefreshCredentials();
        public abstract Task<RefreshResult<CredentialModel>> RefreshCredentialsAsync();

        public SessionCredentialsProvider()
        {
            refreshFunc = RefreshCredentials;
            refreshFuncAsync = RefreshCredentialsAsync;
            credentialsCache = new RefreshCachedSupplier<CredentialModel>(refreshFunc, refreshFuncAsync);
        }


        public CredentialModel GetCredentials()
        {
            return credentialsCache.Get();
        }

        public async Task<CredentialModel> GetCredentialsAsync()
        {
            return await credentialsCache.GetAsync();
        }

        internal static long GetUnixTimeMilliseconds(DateTimeOffset dateTimeOffset)
        {
            DateTimeOffset start = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
            long unixTimeMilliseconds = (long)(dateTimeOffset - start).TotalMilliseconds;
            return unixTimeMilliseconds;
        }

        public long GetStaleTime(long expiration)
        {
            long currentTimeMillis = GetUnixTimeMilliseconds(DateTimeOffset.UtcNow);
            return expiration <= 0 ?
                currentTimeMillis + 60 * 60 * 1000 :
                expiration - 3 * 60 * 1000;
        }

    }
}