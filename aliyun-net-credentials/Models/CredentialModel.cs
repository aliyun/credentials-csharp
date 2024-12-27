using Tea;
using System;

namespace Aliyun.Credentials.Models
{
    public class CredentialModel : TeaModel
    {

        public string AccessKeyId { get; set; }

        public string AccessKeySecret { get; set; }

        public string SecurityToken { get; set; }

        public string BearerToken { get; set; }

        public string Type { get; set; }

        public long Expiration { get; set; }

        public string ProviderName { get; set; }

        public override string ToString()
        {
            return string.Format(
                "Credential(accessKeyId={0}, accessKeySecret={1}, securityToken={2}, providerName={3}, expiration={4})",
                AccessKeyId,
                AccessKeySecret,
                SecurityToken,
                ProviderName,
                Expiration);
        }
    }
}