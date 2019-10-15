using System;

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

        public void RefreshCredential()
        {
            if (WithShouldRefresh())
            {
                RamRoleArnCredential credential = GetNewCredential();
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
                return AuthConstant.RamRoleArn;
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
