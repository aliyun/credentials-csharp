using System;
using System.Threading.Tasks;

using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Utils;

namespace Aliyun.Credentials.Provider
{
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
