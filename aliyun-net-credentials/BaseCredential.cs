using System;
using System.Threading.Tasks;

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
            return DateTime.Now.GetTimeMillis() >= (this.expiration - 180);
        }

        public T GetNewCredential<T>()
        {
            return (T) provider.GetCredentials();
        }

        public async Task<T> GetNewCredentialAsync<T>()
        {
            return (T) await provider.GetCredentialsAsync();
        }
    }
}
