using Tea;

namespace Aliyun.Credentials.Models
{
    public class Config : TeaModel
    {

        [NameInMap("type")]
        public string Type { get; set; }

        [NameInMap("access_key_id")]
        public string AccessKeyId { get; set; }

        [NameInMap("access_key_secret")]
        public string AccessKeySecret { get; set; }

        [NameInMap("role_arn")]
        public string RoleArn { get; set; }

        [NameInMap("role_session_name")]
        public string RoleSessionName { get; set; }

        [NameInMap("public_key_id")]
        public string PublicKeyId { get; set; }

        [NameInMap("role_name")]
        public string RoleName { get; set; }

        [NameInMap("enable_imds_v2")]
        public bool EnableIMDSv2 { get; set; }

        [NameInMap("metadata_token_duration")]
        public int MetadataTokenDuration { get; set; }

        [NameInMap("private_key_file")]
        public string PrivateKeyFile { get; set; }

        [NameInMap("bearer_token")]
        public string BearerToken { get; set; }

        [NameInMap("security_token")]
        public string SecurityToken { get; set; }

        [NameInMap("host")]
        public string Host { get; set; }

        [NameInMap("timeout")]
        public int Timeout { get; set; }

        [NameInMap("connect_timeout")]
        public int ConnectTimeout { get; set; }

        [NameInMap("proxy")]
        public string Proxy { get; set; }

        [NameInMap("policy")]
        public string Policy { get; set; }

        [NameInMap("roleSessionExpiration")]
        public int RoleSessionExpiration { get; set; }
    }
}
