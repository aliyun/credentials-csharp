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
