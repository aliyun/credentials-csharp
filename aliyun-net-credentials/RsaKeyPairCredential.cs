using System.IO;

using Aliyun.Credentials.Provider;
using Aliyun.Credentials.Utils;

namespace Aliyun.Credentials
{
    public class RsaKeyPairCredential : IAlibabaCloudCredentials
    {
        private string privateKeySecret;
        private string publicKeyId;
        private long expiration;
        private IAlibabaCloudCredentialsProvider provider;

        public RsaKeyPairCredential(string publicKeyId, string privateKeySecret, long expiration, IAlibabaCloudCredentialsProvider provider)
        {
            if (publicKeyId == null || privateKeySecret == null)
            {
                throw new InvalidDataException("You must provide a valid pair of Public Key ID and Private Key Secret.");
            }

            this.publicKeyId = publicKeyId;
            this.privateKeySecret = privateKeySecret;
            this.expiration = expiration;
            this.provider = provider;
        }

        public string AccessKeyId
        {
            get
            {
                return this.publicKeyId;
            }
        }

        public string AccessKeySecret
        {
            get
            {
                return this.privateKeySecret;
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
                return AuthConstant.RsaKeyPair;
            }
        }

        public long Expiration
        {
            get
            {
                return this.expiration;
            }
        }
    }
}
