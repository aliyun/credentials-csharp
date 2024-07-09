using System;
using System.Threading.Tasks;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Provider;
using Aliyun.Credentials.Utils;

namespace Aliyun.Credentials
{
    [Obsolete]
    public class EcsRamRoleCredential : BaseCredential, IAlibabaCloudCredentials
    {
        private string accessKeyId;
        private string accessKeySecret;
        private string securityToken;

        public EcsRamRoleCredential(string accessKeyId, string accessKeySecret, string securityToken, long expiration, IAlibabaCloudCredentialsProvider provider) : base(expiration, provider)
        {
            this.accessKeyId = accessKeyId;
            this.accessKeySecret = accessKeySecret;
            this.securityToken = securityToken;
        }

        public void RefreshCredential()
        {
            if (WithShouldRefresh())
            {
                CredentialModel credential = GetNewCredential();
                this.expiration = credential.Expiration;
                this.accessKeyId = credential.AccessKeyId;
                this.accessKeySecret = credential.AccessKeySecret;
                this.securityToken = credential.SecurityToken;
            }
        }

        public async Task RefreshCredentialAsync()
        {
            if (WithShouldRefresh())
            {
                CredentialModel credential = await GetNewCredentialAsync();
                this.expiration = credential.Expiration;
                this.accessKeyId = credential.AccessKeyId;
                this.accessKeySecret = credential.AccessKeySecret;
                this.securityToken = credential.SecurityToken;
            }
        }

        public string GetAccessKeyId()
        {
            RefreshCredential();
            return accessKeyId;
        }

        public async Task<string> GetAccessKeyIdAsync()
        {
            await RefreshCredentialAsync();
            return accessKeyId;
        }

        public string GetAccessKeySecret()
        {
            RefreshCredential();
            return accessKeySecret;
        }

        public async Task<string> GetAccessKeySecretAsync()
        {
            await RefreshCredentialAsync();
            return accessKeySecret;
        }

        public string GetSecurityToken()
        {
            RefreshCredential();
            return securityToken;
        }

        public async Task<string> GetSecurityTokenAsync()
        {
            await RefreshCredentialAsync();
            return securityToken;
        }

        public string GetCredentialType()
        {
            return AuthConstant.EcsRamRole;
        }

        public async Task<string> GetCredentialTypeAsync()
        {
            return await Task.Run(() =>
            {
                return AuthConstant.EcsRamRole;
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
