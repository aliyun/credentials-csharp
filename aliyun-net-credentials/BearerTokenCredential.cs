using System;
using System.Threading.Tasks;

namespace Aliyun.Credentials
{
    [Obsolete]
    public class BearerTokenCredential : BaseCredential, IAlibabaCloudCredentials
    {
        private string bearerToken;
        public BearerTokenCredential(string bearerToken)
        {
            this.bearerToken = bearerToken;
        }

        public string GetAccessKeyId()
        {
            return null;
        }

        public async Task<string> GetAccessKeyIdAsync()
        {
            return await Task.Run(() =>
            {
                string accessKeyId = null;
                return accessKeyId;
            });
        }

        public string GetAccessKeySecret()
        {
            return null;
        }

        public async Task<string> GetAccessKeySecretAsync()
        {
            return await Task.Run(() =>
            {
                string accessKeySecret = null;
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
            return null;
        }

        public async Task<string> GetCredentialTypeAsync()
        {
            return await Task.Run(() =>
            {
                string type = null;
                return type;
            });
        }

        public string GetBearerToken()
        {
            return bearerToken;
        }

        public async Task<string> GetBearerTokenAsync()
        {
            return await Task.Run(() =>
            {
                return bearerToken;
            });
        }

    }
}
