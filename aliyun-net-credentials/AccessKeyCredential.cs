using System;
using System.Threading.Tasks;

using Aliyun.Credentials.Utils;

namespace Aliyun.Credentials
{
    [Obsolete]
    public class AccessKeyCredential : BaseCredential, IAlibabaCloudCredentials
    {
        private string accessKeyId;
        private string accessKeySecret;

        public AccessKeyCredential(string accessKeyId, string accessKeySecret)
        {
            if (accessKeyId == null)
            {
                throw new ArgumentNullException("accessKeyId", "Access key ID cannot be null.");
            }
            if (accessKeySecret == null)
            {
                throw new ArgumentNullException("accessKeySecret", "Access key secret cannot be null.");
            }

            this.accessKeyId = accessKeyId;
            this.accessKeySecret = accessKeySecret;
        }

        public string GetAccessKeyId()
        {
            return accessKeyId;
        }

        public async Task<string> GetAccessKeyIdAsync()
        {
            return await Task.Run(() =>
            {
                return accessKeyId;
            });
        }

        public string GetAccessKeySecret()
        {
            return accessKeySecret;
        }

        public async Task<string> GetAccessKeySecretAsync()
        {
            return await Task.Run(() =>
            {
                return accessKeySecret;
            });
        }

        public string GetSecurityToken()
        {
            return null;
        }

        public async Task<string> GetSecurityTokenAsync()
        {
            return await Task.Run(() =>
            {
                string securityToken = null;
                return securityToken;
            });
        }

        public string GetCredentialType()
        {
            return AuthConstant.AccessKey;
        }

        public async Task<string> GetCredentialTypeAsync()
        {
            return await Task.Run(() =>
            {
                return AuthConstant.AccessKey;
            });
        }
    }
}
