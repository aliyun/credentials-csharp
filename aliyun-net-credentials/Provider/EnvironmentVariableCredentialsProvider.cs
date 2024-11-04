using System;
using System.Threading.Tasks;

using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Utils;

namespace Aliyun.Credentials.Provider
{
    /// <summary>
    /// Look for environment credentials in environment variable.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>If the <c>ALIBABA_CLOUD_ACCESS_KEY_ID</c> and <c>ALIBABA_CLOUD_ACCESS_KEY_SECRET</c> environment variables are defined and are not empty, the program will use them to create default credentials.</description></item>
    /// <item><description>If the <c>ALIBABA_CLOUD_ACCESS_KEY_ID</c>, <c>ALIBABA_CLOUD_ACCESS_KEY_SECRET</c> and <c>ALIBABA_CLOUD_SECURITY_TOKEN</c> environment variables are defined and are not empty, the program will use them to create temporary security credentials(STS). Note: This token has an expiration time, it is recommended to use it in a temporary environment.</description></item>
    /// </list>
    /// </remarks>
    public class EnvironmentVariableCredentialsProvider : IAlibabaCloudCredentialsProvider
    {
        public CredentialModel GetCredentials()
        {
            string accessKeyId = AuthUtils.EnvironmentAccessKeyId;
            string accessKeySecret = AuthUtils.EnvironmentAccesskeySecret;
            string securityToken = AuthUtils.EnvironmentSecurityToken;
            if (string.IsNullOrWhiteSpace(accessKeyId))
            {
                throw new CredentialException("Environment variable accessKeyId cannot be empty");
            }
            else if (string.IsNullOrWhiteSpace(accessKeySecret))
            {
                throw new CredentialException("Environment variable accessKeySecret cannot be empty");
            }

            if (!string.IsNullOrWhiteSpace(securityToken))
            {
                return new CredentialModel
                {
                    AccessKeyId = accessKeyId,
                    AccessKeySecret = accessKeySecret,
                    SecurityToken = securityToken,
                    Type = AuthConstant.Sts,
                    ProviderName = GetProviderName()
                };
            }

            return new CredentialModel
            {
                AccessKeyId = accessKeyId,
                AccessKeySecret = accessKeySecret,
                Type = AuthConstant.AccessKey,
                ProviderName = GetProviderName()
            };
        }

        public async Task<CredentialModel> GetCredentialsAsync()
        {
            return await Task.Run(() =>
            {
                return GetCredentials();
            });
        }

        public string GetProviderName()
        {
            return "env";
        }
    }
}
