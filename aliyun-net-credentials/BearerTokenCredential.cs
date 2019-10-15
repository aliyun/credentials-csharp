namespace Aliyun.Credentials
{
    public class BearerTokenCredential : IAlibabaCloudCredentials
    {
        public BearerTokenCredential(string bearerToken)
        {
            BearerToken = bearerToken;
        }

        public string AccessKeyId
        {
            get
            {
                return null;
            }
        }

        public string AccessKeySecret
        {
            get
            {
                return null;
            }
        }

        public string SecurityToken
        {
            get
            {
                return null;
            }
        }

        public string CredentialType
        {
            get
            {
                return null;
            }
        }

        public string BearerToken { get; set; }

    }
}
