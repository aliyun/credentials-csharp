using System.IO;
using System.Threading.Tasks;

using Aliyun.Credentials.Provider;
using Aliyun.Credentials.Utils;

namespace Aliyun.Credentials
{
    public class RsaKeyPairCredential : IAlibabaCloudCredentials
    {
        private string privateKeySecret;
        private string publicKeyId;
        private long expiration;
        private IAlibabaCloudCredentialsProvider provider;

        public RsaKeyPairCredential(string publicKeyId, string privateKeySecret, long expiration, IAlibabaCloudCredentialsProvider provider)
        {
            if (publicKeyId == null || privateKeySecret == null)
            {
                throw new InvalidDataException("You must provide a valid pair of Public Key ID and Private Key Secret.");
            }

            this.publicKeyId = publicKeyId;
            this.privateKeySecret = privateKeySecret;
            this.expiration = expiration;
            this.provider = provider;
        }

        public string GetAccessKeyId()
        {
            return publicKeyId;
        }

        public async Task<string> GetAccessKeyIdAsync()
        {
            return await Task.Run(() =>
            {
                return publicKeyId;
            });
        }

        public string GetAccessKeySecret()
        {
            return privateKeySecret;
        }

        public async Task<string> GetAccessKeySecretAsync()
        {
            return await Task.Run(() =>
            {
                return privateKeySecret;
            });
        }

        public string GetSecurityToken()
        {
            return null;
        }

        public async Task<string> GetSecurityTokenAsync()
        {
            return await Task.Run(() =>
            {
                string securityToken = null;
                return securityToken;
            });
        }

        public string GetCredentialType()
        {
            return AuthConstant.RsaKeyPair;
        }

        public async Task<string> GetCredentialTypeAsync()
        {
            return await Task.Run(() =>
            {
                return AuthConstant.RsaKeyPair;
            });
        }

        public long GetExpiration()
        {
            return expiration;
        }

        public async Task<long> GetExpirationAsync()
        {
            return await Task.Run(() =>
            {
                return expiration;
            });
        }
    }
}
