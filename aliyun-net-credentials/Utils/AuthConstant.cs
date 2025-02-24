using System;

namespace Aliyun.Credentials.Utils
{
    public class AuthConstant
    {
        public const string EnvAccessKeyId = Configure.Constants.EnvPrefix + "ACCESS_KEY_ID";
        public const string EnvAccessKeySecret = Configure.Constants.EnvPrefix + "ACCESS_KEY_SECRET";
        public const string EnvSecurityToken = Configure.Constants.EnvPrefix + "SECURITY_TOKEN";
        public const string IniAccessKeyId = "access_key_id";
        public const string IniAccessKeyIdsecret = "access_key_secret";
        public const string IniType = "type";
        public const string IniTypeRam = "ecs_ram_role";
        public const string IniTypeArn = "ram_role_arn";
        public const string IniTypeOIDC = "oidc_role_arn";
        public const string IniTypeKeyPair = "rsa_key_pair";
        public const string IniPublicKeyId = "public_key_id";
        public const string IniPrivateKeyFile = "private_key_file";
        public const string IniPrivateKey = "private_key";
        public const string IniRoleName = "role_name";
        public const string IniRoleSessionName = "role_session_name";
        public const string IniRoleArn = "role_arn";
        public const string IniPolicy = "policy";
        public const string IniOIDCProviderArn = "oidc_provider_arn";
        public const string IniOIDCTokenFilePath = "oidc_token_file_path";
        public const string DefaultRegion = "region_id";
        public const string iniStsRegionId = "sts_region";
        public const string iniExternalId = "external_id";
        public const string iniEnableVpc = "enable_vpc";
        public const string IniEnable = "enable";

        public const string AccessKey = "access_key";
        public const string Sts = "sts";
        public const string EcsRamRole = "ecs_ram_role";
        public const string RamRoleArn = "ram_role_arn";
        public const string RsaKeyPair = "rsa_key_pair";
        public const string BeareaToken = "bearer";
        public const string OIDCRoleArn = "oidc_role_arn";
        [Obsolete("use CredentialsURI instead")]
        public const string URLSts = "credentials_uri";
        public const string CredentialsURI = "credentials_uri";
        public const string StaticSts = "static_sts";
        public const string StaticAK = "static_ak";

        private static string GetHomePath()
        {
            return Environment.OSVersion.Platform == PlatformID.Unix ?
                Environment.GetEnvironmentVariable("HOME") :
                Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
        }

        private static string GetOsSlash()
        {
            return Environment.OSVersion.Platform == PlatformID.Unix ? "/" : "\\";
        }

        public static string GetDefaultFilePath()
        {
            string homePath = GetHomePath();
            string slash = GetOsSlash();
            return homePath + slash + Configure.Constants.PATHCredentialFile + slash + "credentials.ini";
        }
    }
}
