using System.Threading.Tasks;

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
            if (null == config)
            {
                DefaultCredentialsProvider provider = new DefaultCredentialsProvider();
                this.cloudCredential = provider.GetCredentials();
                return;
            }
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
                case AuthConstant.BeareaToken:
                    return new BearerTokenCredential(config.BearerToken);
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
            return cloudCredential.GetAccessKeyId();
        }

        public async Task<string> GetAccessKeyIdAsync()
        {
            return await cloudCredential.GetAccessKeyIdAsync();
        }

        public string GetAccessKeySecret()
        {
            return cloudCredential.GetAccessKeySecret();
        }

        public async Task<string> GetAccessKeySecretAsync()
        {
            return await cloudCredential.GetAccessKeySecretAsync();
        }

        public string GetSecurityToken()
        {
            return cloudCredential.GetSecurityToken();
        }

        public async Task<string> GetSecurityTokenAsync()
        {
            return await cloudCredential.GetSecurityTokenAsync();
        }

        public string GetBearerToken()
        {
            if (cloudCredential is BearerTokenCredential)
            {
                return (((BearerTokenCredential) cloudCredential).GetBearerToken());
            }
            return null;
        }

        public async Task<string> GetBearerTokenAsync()
        {
            if (cloudCredential is BearerTokenCredential)
            {
                return await ((BearerTokenCredential) cloudCredential).GetBearerTokenAsync();
            }
            return null;
        }

        public new string GetType()
        {
            return cloudCredential.GetCredentialType();
        }

        public async Task<string> GetTypeAsync()
        {
            return await cloudCredential.GetCredentialTypeAsync();
        }
    }
}
