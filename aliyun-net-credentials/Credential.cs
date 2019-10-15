using Aliyun.Credentials.Provider;
using Aliyun.Credentials.Utils;

namespace Aliyun.Credentials
{
    public class Credential
    {
        private IAlibabaCloudCredentials cloudCredential;

        public Credential(Configuration config)
        {
            this.cloudCredential = GetCredential(config);
        }

        private IAlibabaCloudCredentials GetCredential(Configuration config)
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

        private IAlibabaCloudCredentialsProvider GetProvider(Configuration config)
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

        public string AccessKeyId
        {
            get
            {
                return this.cloudCredential.AccessKeyId;
            }
        }

        public string AccessKeySecret
        {
            get
            {
                return this.cloudCredential.AccessKeySecret;
            }
        }

        public string SecurityToken
        {
            get
            {
                return this.cloudCredential.SecurityToken;
            }
        }

        public string Type
        {
            get
            {
                return this.cloudCredential.CredentialType;
            }
        }
    }
}
