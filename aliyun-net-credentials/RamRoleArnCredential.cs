using System;
using System.Threading.Tasks;

using Aliyun.Credentials.Provider;
using Aliyun.Credentials.Utils;

namespace Aliyun.Credentials
{
    public class RamRoleArnCredential : IAlibabaCloudCredentials
    {
        private string accessKeyId;
        private string accessKeySecret;
        private string securityToken;
        private long expiration;
        private IAlibabaCloudCredentialsProvider provider;

        public RamRoleArnCredential(string accessKeyId, string accessKeySecret, string securityToken, long expiration,
            IAlibabaCloudCredentialsProvider provider)
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

        public RamRoleArnCredential GetNewCredential()
        {
            return (RamRoleArnCredential) provider.GetCredentials();
        }

        public async Task<RamRoleArnCredential> GetNewCredentialAsync()
        {
            return (RamRoleArnCredential) await provider.GetCredentialsAsync();
        }

        public void RefreshCredential()
        {
            if (WithShouldRefresh())
            {
                RamRoleArnCredential credential = GetNewCredential();
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
                RamRoleArnCredential credential = await GetNewCredentialAsync();
                this.expiration = await credential.GetExpirationAsync();
                this.accessKeyId = await credential.GetAccessKeyIdAsync();
                this.accessKeySecret = await credential.GetAccessKeySecretAsync();
                this.securityToken = await credential.GetSecurityTokenAsync();
            }
        }

        public string GetAccessKeyId()
        {
            RefreshCredential();
            return this.accessKeyId;
        }

        public async Task<string> GetAccessKeyIdAsync()
        {
            await RefreshCredentialAsync();
            return this.accessKeyId;
        }

        public string GetAccessKeySecret()
        {
            RefreshCredential();
            return this.accessKeySecret;
        }

        public async Task<string> GetAccessKeySecretAsync()
        {
            await RefreshCredentialAsync();
            return this.accessKeySecret;
        }

        public string GetSecurityToken()
        {
            RefreshCredential();
            return this.securityToken;
        }

        public async Task<string> GetSecurityTokenAsync()
        {
            await RefreshCredentialAsync();
            return this.securityToken;
        }

        public string GetCredentialType()
        {
            return AuthConstant.RamRoleArn;
        }

        public async Task<string> GetCredentialTypeAsync()
        {
            return await Task.Run(() =>
            {
                return AuthConstant.RamRoleArn;
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
