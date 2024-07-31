using System;

namespace Aliyun.Credentials.Utils
{
    public class AuthConstant
    {
        public const string EnvAccessKeyId = "ALIBABA_CLOUD_ACCESS_KEY_ID";
        public const string EnvAccessKeySecret = "ALIBABA_CLOUD_ACCESS_KEY_SECRET";
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
        public const string IniEnable = "enable";

        public const string AccessKey = "access_key";
        public const string Sts = "sts";
        public const string EcsRamRole = "ecs_ram_role";
        public const string RamRoleArn = "ram_role_arn";
        public const string RsaKeyPair = "rsa_key_pair";
        public const string BeareaToken = "bearer";
        public const string OIDCRoleArn = "oidc_role_arn";
        public const string URLSts = "credentials_uri";

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
            return homePath + slash + ".alibabacloud" + slash + "credentials.ini";
        }
    }
}
