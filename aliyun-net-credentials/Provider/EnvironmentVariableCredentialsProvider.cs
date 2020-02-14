using System.Threading.Tasks;

using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Utils;

namespace Aliyun.Credentials.Provider
{
    public class EnvironmentVariableCredentialsProvider : IAlibabaCloudCredentialsProvider
    {
        public IAlibabaCloudCredentials GetCredentials()
        {
            if (AuthUtils.ClientType != "default")
            {
                return null;
            }

            string accessKeyId = AuthUtils.EnvironmentAccessKeyId;
            string accessKeySecret = AuthUtils.EnvironmentAccesskeySecret;
            if (accessKeyId == null || accessKeySecret == null)
            {
                return null;
            }
            else if (string.IsNullOrWhiteSpace(accessKeyId))
            {
                throw new CredentialException("Environment variable accessKeyId cannot be empty");
            }
            else if (string.IsNullOrWhiteSpace(accessKeySecret))
            {
                throw new CredentialException("Environment variable accessKeySecret cannot be empty");
            }

            return new AccessKeyCredential(accessKeyId, accessKeySecret);
        }

        public async Task<IAlibabaCloudCredentials> GetCredentialsAsync()
        {
            return await Task.Run(() =>
            {
                return GetCredentials();
            });
        }
    }
}
