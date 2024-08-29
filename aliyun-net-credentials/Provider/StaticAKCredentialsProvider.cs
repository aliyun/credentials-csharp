using System;
using System.Threading.Tasks;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Utils;

namespace Aliyun.Credentials.Provider
{
    internal class StaticAKCredentialsProvider : IAlibabaCloudCredentialsProvider
    {
        private readonly string accessKeyId;
        private readonly string accessKeySecret;

        public StaticAKCredentialsProvider(Config config)
        {
            accessKeyId = ParameterHelper.ValidateEnvNotNull(config.AccessKeyId, "ALIBABA_CLOUD_ACCESS_KEY_ID", "AccessKeyId", "AccessKeyId must not be null.");
            accessKeySecret = ParameterHelper.ValidateEnvNotNull(config.AccessKeySecret, "ALIBABA_CLOUD_ACCESS_KEY_SECRET", "AccessKeySecret", "AccessKeySecret must not be null.");
        }

        public StaticAKCredentialsProvider(string accessKeyId, string accessKeySecret)
        {
            this.accessKeyId = ParameterHelper.ValidateEnvNotNull(accessKeyId, "ALIBABA_CLOUD_ACCESS_KEY_ID", "AccessKeyId", "AccessKeyId must not be null.");
            this.accessKeySecret = ParameterHelper.ValidateEnvNotNull(accessKeySecret, "ALIBABA_CLOUD_ACCESS_KEY_SECRET", "AccessKeySecret", "AccessKeySecret must not be null.");
        }

        public CredentialModel GetCredentials()
        {
            return new CredentialModel
            {
                AccessKeyId = accessKeyId,
                AccessKeySecret = accessKeySecret,
                ProviderName = GetProviderName()
            };
        }

        public Task<CredentialModel> GetCredentialsAsync()
        {
            return Task.Run(() =>
            {
                return new CredentialModel
            {
                AccessKeyId = accessKeyId,
                AccessKeySecret = accessKeySecret,
                ProviderName = GetProviderName()
            };
            });
        }

        public string GetProviderName()
        {
            return AuthConstant.StaticAK;
        }
    }
}
