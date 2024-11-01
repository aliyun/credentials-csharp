using System.Threading.Tasks;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Utils;

namespace Aliyun.Credentials.Provider
{
    internal class StaticSTSCredentialsProvider : IAlibabaCloudCredentialsProvider
    {
        private readonly string accessKeyId;
        private readonly string accessKeySecret;
        private readonly string securityToken;

        public StaticSTSCredentialsProvider(Config config)
        {
            securityToken = ParameterHelper.ValidateEnvNotNull(config.SecurityToken, "ALIBABA_CLOUD_SECURITY_TOKEN", "SecurityToken", "SecurityToken must not be null.");
            accessKeyId = ParameterHelper.ValidateEnvNotNull(config.AccessKeyId, "ALIBABA_CLOUD_ACCESS_KEY_ID", "AccessKeyId", "AccessKeyId must not be null.");
            accessKeySecret = ParameterHelper.ValidateEnvNotNull(config.AccessKeySecret, "ALIBABA_CLOUD_ACCESS_KEY_SECRET", "AccessKeySecret", "AccessKeySecret must not be null.");
        }

        public StaticSTSCredentialsProvider(Builder builder)
        {
            this.securityToken = ParameterHelper.ValidateEnvNotNull(builder.securityToken, "ALIBABA_CLOUD_SECURITY_TOKEN", "SecurityToken", "SecurityToken must not be null.");
            this.accessKeyId = ParameterHelper.ValidateEnvNotNull(builder.accessKeyId, "ALIBABA_CLOUD_ACCESS_KEY_ID", "AccessKeyId", "AccessKeyId must not be null.");
            this.accessKeySecret = ParameterHelper.ValidateEnvNotNull(builder.accessKeySecret, "ALIBABA_CLOUD_ACCESS_KEY_SECRET", "AccessKeySecret", "AccessKeySecret must not be null.");
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

            public Builder SecurityToken(string securityToken)
            {
                this.securityToken = securityToken;
                return this;
            }

            public StaticSTSCredentialsProvider Build()
            {
                return new StaticSTSCredentialsProvider(this);
            } 
        }

        public CredentialModel GetCredentials()
        {
            return new CredentialModel
            {
                AccessKeyId = accessKeyId,
                AccessKeySecret = accessKeySecret,
                SecurityToken = securityToken,
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
                    SecurityToken = securityToken,
                    ProviderName = GetProviderName()
                };
            });
        }

        public string GetProviderName()
        {
            return AuthConstant.StaticSts;
        }
    }
}
