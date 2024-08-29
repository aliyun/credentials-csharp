using Tea;

namespace  Aliyun.Credentials.Models
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
    }
}