using System;
using System.Threading.Tasks;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Utils;

namespace Aliyun.Credentials.Provider
{
    /// <summary>
    /// <para>Setup access_key credential through <see href="https://usercenter.console.aliyun.com/#/manage/ak">User Information Management</see>, it have full authority over the account, please keep it safe.</para>
    /// <para>Sometimes for security reasons, you cannot hand over a primary account AccessKey with full access to the developer of a project. You may create a sub-account <see href="https://ram.console.aliyun.com/users">RAM Sub-account</see>, grant its <see href="https://ram.console.aliyun.com/permissions">authorization</see>，and use the AccessKey of RAM Sub-account.</para>
    /// </summary>
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
