using Aliyun.Credentials.Models;
using Aliyun.Credentials.Provider;
using Aliyun.Credentials.Utils;

namespace Aliyun.Credentials
{
    public class Client
    {
        private IAlibabaCloudCredentials cloudCredential;

        public Client(Config config)
        {
            this.cloudCredential = GetCredential(config);
        }

        private IAlibabaCloudCredentials GetCredential(Config config)
        {
            switch (config.Type)
            {
                case AuthConstant.AccessKey:
                    return new AccessKeyCredential(config.AccessKeyId, config.AccessKeySecret);
                case AuthConstant.Sts:
                    return new StsCredential(config.AccessKeyId, config.AccessKeySecret, config.SecurityToken);
                default:
                    return this.GetProvider(config).GetCredentials();
            }
        }

        private IAlibabaCloudCredentialsProvider GetProvider(Config config)
        {
            switch (config.Type)
            {
                case AuthConstant.EcsRamRole:
                    return new EcsRamRoleCredentialProvider(config);
                case AuthConstant.RamRoleArn:
                    return new RamRoleArnCredentialProvider(config);
                case AuthConstant.RsaKeyPair:
                    return new RsaKeyPairCredentialProvider(config);
                default:
                    return new DefaultCredentialsProvider();
            }
        }

        public string GetAccessKeyId()
        {
            return this.cloudCredential.AccessKeyId;
        }

        public string GetAccessKeySecret()
        {
            return this.cloudCredential.AccessKeySecret;
        }

        public string GetSecurityToken()
        {
            return this.cloudCredential.SecurityToken;
        }

        public string GetBearerToken()
        {
            if (this.cloudCredential is BearerTokenCredential)
            {
                return (((BearerTokenCredential) this.cloudCredential).BearerToken);
            }
            return string.Empty;
        }

        public new string GetType()
        {
            return this.cloudCredential.CredentialType;
        }
    }
}
