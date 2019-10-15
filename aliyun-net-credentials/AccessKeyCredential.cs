using System;

using Aliyun.Credentials.Utils;

namespace Aliyun.Credentials
{
    public class AccessKeyCredential : IAlibabaCloudCredentials
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

        public string AccessKeyId
        {
            get
            {
                return this.accessKeyId;
            }
        }

        public string AccessKeySecret
        {
            get
            {
                return this.accessKeySecret;
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
                return AuthConstant.AccessKey;
            }
        }

    }
}
