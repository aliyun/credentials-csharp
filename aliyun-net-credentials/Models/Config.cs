using Tea;

namespace Aliyun.Credentials.Models
{
    public class Config : TeaModel
    {

        /// <summary>
        /// Credential type
        /// </summary>
        /// <example>
        /// access_key, sts, bearer, ecs_ram_role, ram_role_arn, rsa_key_pair, oidc_role_arn, credentials_uri
        /// </example>
        [NameInMap("type")]
        public string Type { get; set; }

        [NameInMap("access_key_id")]
        public string AccessKeyId { get; set; }

        [NameInMap("access_key_secret")]
        public string AccessKeySecret { get; set; }

        /// <summary>
        /// The ARN of the RAM role to be assumed.
        /// </summary>
        /// <example>
        /// acs:ram::123456789012****:role/adminrole.
        /// </example>
        [NameInMap("role_arn")]
        public string RoleArn { get; set; }

        /// <summary>
        /// The name of the role session.
        /// </summary>
        [NameInMap("role_session_name")]
        public string RoleSessionName { get; set; }

        [NameInMap("public_key_id")]
        public string PublicKeyId { get; set; }

        /// <summary>
        /// The name of the RAM role of the ECS instance.
        /// </summary>
        [NameInMap("role_name")]
        public string RoleName { get; set; }

        /// <summary>
        /// Whether fallback to IMDS v1 is disabled. Default: false.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item><description>false: Try to obtain credentials in security hardening mode first. If failed, switch to normal mode and try again (IMDSv1)</description></item>
        /// <item><description>true: Force to obtain credentials in security hardening mode</description></item>
        /// </list>
        /// </remarks>
        [NameInMap("disableIMDSv1")]
        public bool? DisableIMDSv1 { get; set; }

        [NameInMap("private_key_file")]
        public string PrivateKeyFile { get; set; }

        [NameInMap("bearer_token")]
        public string BearerToken { get; set; }

        [NameInMap("security_token")]
        public string SecurityToken { get; set; }

        [NameInMap("host")]
        public string Host { get; set; }

        /// <summary>
        /// Read timeout for network requests. Default 1s for EcsRamRoleCredentialsProvider and 10s for other providers.
        /// </summary>
        [NameInMap("timeout")]
        public int Timeout { get; set; }

        /// <summary>
        /// Connect timeout for network requests. Default 1s for EcsRamRoleCredentialsProvider and 5s for other providers.
        /// </summary>
        [NameInMap("connect_timeout")]
        public int ConnectTimeout { get; set; }

        [NameInMap("proxy")]
        public string Proxy { get; set; }

        /// <summary>
        /// Limited permissions for the RAM role.
        /// </summary>
        /// <example>
        /// {"Statement": [{"Action": ["*"],"Effect": "Allow","Resource": ["*"]}],"Version":"1"}
        /// </example>
        [NameInMap("policy")]
        public string Policy { get; set; }

        /// <summary>
        /// The validity period of the role session. Unit of Second. Default 3600s.
        /// </summary>
        [NameInMap("roleSessionExpiration")]
        public int RoleSessionExpiration { get; set; }

        /// <summary>
        /// The ARN of the OIDC IdP.
        /// </summary>
        [NameInMap("oidcProviderArn")]
        public string OIDCProviderArn { get; set; }

        /// <summary>
        /// The path of the OIDC token file.
        /// </summary>
        [NameInMap("oidcTokenFilePath")]
        public string OIDCTokenFilePath { get; set; }

        /// <summary>
        /// The URI of the credential in the http://local_or_remote_uri/ format.
        /// </summary>
        [NameInMap("credentialsURI")]
        public string CredentialsURI { get; set; }

        /// <summary>
        /// Endpoint of STS. Default sts.aliyuncs.com.
        /// </summary>
        /// <example>
        /// sts.cn-hangzhou.aliyuncs.com, sts-vpc.cn-hangzhou.aliyuncs.com
        /// </example>
        [NameInMap("stsEndpoint")]
        public string STSEndpoint { get; set; }

        /// <summary>
        /// The external ID of the RAM role.
        /// This parameter is provided by an external party and is used to prevent the confused deputy problem.
        /// </summary>
        [NameInMap("externalId")]
        public string ExternalId { get; set; }
    }
}
