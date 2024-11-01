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
            accessKeyId = ParameterHelper.ValidateEnvNotEmpty(config.AccessKeyId, "ALIBABA_CLOUD_ACCESS_KEY_ID", "AccessKeyId", "AccessKeyId must not be null or empty.");
            accessKeySecret = ParameterHelper.ValidateEnvNotEmpty(config.AccessKeySecret, "ALIBABA_CLOUD_ACCESS_KEY_SECRET", "AccessKeySecret", "AccessKeySecret must not be null or empty.");
        }

        public StaticAKCredentialsProvider(string accessKeyId, string accessKeySecret)
        {
            this.accessKeyId = ParameterHelper.ValidateEnvNotEmpty(accessKeyId, "ALIBABA_CLOUD_ACCESS_KEY_ID", "AccessKeyId", "AccessKeyId must not be null or empty.");
            this.accessKeySecret = ParameterHelper.ValidateEnvNotEmpty(accessKeySecret, "ALIBABA_CLOUD_ACCESS_KEY_SECRET", "AccessKeySecret", "AccessKeySecret must not be null or empty.");
        }

        public StaticAKCredentialsProvider(Builder builder)
        {
            this.accessKeyId = ParameterHelper.ValidateEnvNotEmpty(builder.accessKeyId, "ALIBABA_CLOUD_ACCESS_KEY_ID", "AccessKeyId", "AccessKeyId must not be null or empty.");
            this.accessKeySecret = ParameterHelper.ValidateEnvNotEmpty(builder.accessKeySecret, "ALIBABA_CLOUD_ACCESS_KEY_SECRET", "AccessKeySecret", "AccessKeySecret must not be null or empty.");
        }

        public class Builder
        {
            internal string securityToken;
            internal string accessKeyId;
            internal string accessKeySecret;

            public Builder AccessKeyId(string accessKeyId)
            {
                this.accessKeyId = accessKeyId;
                return this;
            }

            public Builder AccessKeySecret(string accessKeySecret)
            {
                this.accessKeySecret = accessKeySecret;
                return this;
            }

            public StaticAKCredentialsProvider Build()
            {
                return new StaticAKCredentialsProvider(this);
            } 

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
