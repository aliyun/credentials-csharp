using System;

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

        public void RefreshCredential()
        {
            if (WithShouldRefresh())
            {
                EcsRamRoleCredential credential = GetNewCredential();
                this.expiration = credential.Expiration;
                this.accessKeyId = credential.AccessKeyId;
                this.accessKeySecret = credential.AccessKeySecret;
                this.securityToken = credential.SecurityToken;
            }
        }

        public string AccessKeyId
        {
            get
            {
                RefreshCredential();
                return this.accessKeyId;
            }
        }

        public string AccessKeySecret
        {
            get
            {
                RefreshCredential();
                return this.accessKeySecret;
            }
        }

        public string SecurityToken
        {
            get
            {
                RefreshCredential();
                return this.securityToken;
            }
        }

        public string CredentialType
        {
            get
            {
                return AuthConstant.EcsRamRole;
            }
        }

        public long Expiration
        {
            get
            {
                RefreshCredential();
                return expiration;
            }
        }
    }
}
