using System;
using System.IO;
using System.Threading.Tasks;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Provider;
using Aliyun.Credentials.Utils;

namespace Aliyun.Credentials
{
    [Obsolete]
    public class RsaKeyPairCredential : BaseCredential, IAlibabaCloudCredentials
    {
        private string privateKeySecret;
        private string publicKeyId;

        public RsaKeyPairCredential(string publicKeyId, string privateKeySecret, long expiration, IAlibabaCloudCredentialsProvider provider) : base(expiration, provider)
        {
            if (publicKeyId == null || privateKeySecret == null)
            {
                throw new InvalidDataException("You must provide a valid pair of Public Key ID and Private Key Secret.");
            }

            this.publicKeyId = publicKeyId;
            this.privateKeySecret = privateKeySecret;
        }

        public void RefreshCredential()
        {
            if (WithShouldRefresh())
            {
                CredentialModel credential = GetNewCredential();
                this.expiration = credential.Expiration;
                this.publicKeyId = credential.AccessKeyId;
                this.privateKeySecret = credential.AccessKeySecret;
            }
        }
        public async Task RefreshCredentialAsync()
        {
            if (WithShouldRefresh())
            {
                CredentialModel credential = await GetNewCredentialAsync();
                this.expiration = credential.Expiration;
                this.publicKeyId = credential.AccessKeyId;
                this.privateKeySecret = credential.AccessKeySecret;
            }
        }

        public string GetAccessKeyId()
        {
            RefreshCredential();
            return publicKeyId;
        }

        public async Task<string> GetAccessKeyIdAsync()
        {
            await RefreshCredentialAsync();
            return publicKeyId;
        }

        public string GetAccessKeySecret()
        {
            RefreshCredential();
            return privateKeySecret;
        }

        public async Task<string> GetAccessKeySecretAsync()
        {
            await RefreshCredentialAsync();
            return privateKeySecret;
        }

        public string GetSecurityToken()
        {
            return null;
        }

        public async Task<string> GetSecurityTokenAsync()
        {
            await RefreshCredentialAsync();
            return null;
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
            RefreshCredential();
            return expiration;
        }

        public async Task<long> GetExpirationAsync()
        {
            await RefreshCredentialAsync();
            return expiration;
        }
    }
}
