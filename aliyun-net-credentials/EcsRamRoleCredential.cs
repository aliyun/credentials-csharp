using System;
using System.Threading.Tasks;

using Aliyun.Credentials.Provider;
using Aliyun.Credentials.Utils;

namespace Aliyun.Credentials
{
    public class EcsRamRoleCredential : IAlibabaCloudCredentials
    {
        private long expiration;
        private string accessKeyId;
        private string accessKeySecret;
        private string securityToken;
        private IAlibabaCloudCredentialsProvider provider;

        public EcsRamRoleCredential(string accessKeyId, string accessKeySecret, string securityToken, long expiration, IAlibabaCloudCredentialsProvider provider)
        {
            this.accessKeyId = accessKeyId;
            this.accessKeySecret = accessKeySecret;
            this.securityToken = securityToken;
            this.expiration = expiration;
            this.provider = provider;
        }

        public bool WithShouldRefresh()
        {
            return DateTime.Now.GetTimeMillis() >= (this.expiration - 180);
        }

        public EcsRamRoleCredential GetNewCredential()
        {
            return (EcsRamRoleCredential) provider.GetCredentials();
        }

        public async Task<EcsRamRoleCredential> GetNewCredentialAsync()
        {
            return (EcsRamRoleCredential) await provider.GetCredentialsAsync();
        }

        public void RefreshCredential()
        {
            if (WithShouldRefresh())
            {
                EcsRamRoleCredential credential = GetNewCredential();
                this.expiration = credential.GetExpiration();
                this.accessKeyId = credential.GetAccessKeyId();
                this.accessKeySecret = credential.GetAccessKeySecret();
                this.securityToken = credential.GetSecurityToken();
            }
        }

        public async Task RefreshCredentialAsync()
        {
            if (WithShouldRefresh())
            {
                EcsRamRoleCredential credential = await GetNewCredentialAsync();
                this.expiration = await credential.GetExpirationAsync();
                this.accessKeyId = await credential.GetAccessKeyIdAsync();
                this.accessKeySecret = await credential.GetAccessKeySecretAsync();
                this.securityToken = await credential.GetSecurityTokenAsync();
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
