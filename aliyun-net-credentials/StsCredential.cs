using Aliyun.Credentials.Utils;

namespace Aliyun.Credentials
{
    public class StsCredential : IAlibabaCloudCredentials
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

        public string AccessKeyId
        {
            get { return accessKeyId; }
        }

        public string AccessKeySecret
        {
            get { return accessKeySecret; }
        }

        public string SecurityToken
        {
            get { return securityToken; }
        }

        public string CredentialType
        {
            get { return AuthConstant.Sts; }
        }
    }
}
