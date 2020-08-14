using System.Threading.Tasks;

using Aliyun.Credentials.Utils;

namespace Aliyun.Credentials
{
    public class StsCredential : BaseCredential, IAlibabaCloudCredentials
    {
        private readonly string accessKeyId;
        private readonly string accessKeySecret;
        private readonly string securityToken;

        public StsCredential()
        {

        }

        public StsCredential(string accessKeyId, string accessKeySecret, string securityToken)
        {
            this.accessKeyId = accessKeyId;
            this.accessKeySecret = accessKeySecret;
            this.securityToken = securityToken;
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
            return securityToken;
        }

        public async Task<string> GetSecurityTokenAsync()
        {
            return await Task.Run(() =>
            {
                return securityToken;
            });
        }

        public string GetCredentialType()
        {
            return AuthConstant.Sts;
        }

        public async Task<string> GetCredentialTypeAsync()
        {
            return await Task.Run(() =>
            {
                return AuthConstant.Sts;
            });
        }
    }
}
