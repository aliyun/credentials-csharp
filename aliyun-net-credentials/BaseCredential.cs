using System;
using System.Threading.Tasks;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Provider;
using Aliyun.Credentials.Utils;

namespace Aliyun.Credentials
{
    public class BaseCredential
    {
        protected long expiration;
        protected IAlibabaCloudCredentialsProvider provider;

        public BaseCredential()
        {

        }

        public BaseCredential(long expiration, IAlibabaCloudCredentialsProvider provider)
        {
            this.expiration = expiration;
            this.provider = provider;
        }

        public bool WithShouldRefresh()
        {
            return DateTime.UtcNow.GetTimeMillis() >= (this.expiration - 180 * 1000);
        }

        public CredentialModel GetNewCredential()
        {
            return provider.GetCredentials();
        }

        public async Task<CredentialModel> GetNewCredentialAsync()
        {
            return await provider.GetCredentialsAsync();
        }
    }
}
