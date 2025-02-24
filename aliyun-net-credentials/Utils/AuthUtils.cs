using System;
using System.Text;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using Aliyun.Credentials.Exceptions;

namespace Aliyun.Credentials.Utils
{
    public class AuthUtils
    {
        private string clientType;
        private string environmentAccessKeyId;
        private string environmentAccesskeySecret;
        private string environmentSecurityToken;
        private string environmentEcsMetaData;
        private string environmentEcsMetaDataDisabled;
        private string environmentCredentialsFile;
        private string environmentRoleArn;
        private string environmentOIDCProviderArn;
        private string environmentOIDCTokenFilePath;
        private string environmentCLIProfileDisabled;
        private volatile string environmentCredentialsURI;
        private volatile string disableECSIMDSv1;
        private string privateKey;
        private static volatile string oidcToken;

        static AuthUtils authUtils = new AuthUtils();

        AuthUtils()
        {
            clientType = Environment.GetEnvironmentVariable(Configure.Constants.EnvPrefix + "PROFILE") ?? clientType;
            environmentAccessKeyId = Environment.GetEnvironmentVariable(Configure.Constants.EnvPrefix + "ACCESS_KEY_ID") ?? environmentAccessKeyId;
            environmentAccesskeySecret = Environment.GetEnvironmentVariable(Configure.Constants.EnvPrefix + "ACCESS_KEY_SECRET") ?? environmentAccesskeySecret;
            environmentSecurityToken = Environment.GetEnvironmentVariable(Configure.Constants.EnvPrefix + "SECURITY_TOKEN") ?? environmentSecurityToken;
            environmentEcsMetaData = Environment.GetEnvironmentVariable(Configure.Constants.EnvPrefix + "ECS_METADATA") ?? environmentEcsMetaData;
            environmentEcsMetaDataDisabled = Environment.GetEnvironmentVariable(Configure.Constants.EnvPrefix + "ECS_METADATA_DISABLED") ?? environmentEcsMetaDataDisabled;
            environmentCredentialsFile = Environment.GetEnvironmentVariable(Configure.Constants.EnvPrefix + "CREDENTIALS_FILE") ?? environmentCredentialsFile;
            environmentRoleArn = Environment.GetEnvironmentVariable(Configure.Constants.EnvPrefix + "ROLE_ARN") ?? environmentRoleArn;
            environmentOIDCProviderArn = Environment.GetEnvironmentVariable(Configure.Constants.EnvPrefix + "OIDC_PROVIDER_ARN") ?? environmentOIDCProviderArn;
            environmentOIDCTokenFilePath = Environment.GetEnvironmentVariable(Configure.Constants.EnvPrefix + "OIDC_TOKEN_FILE") ?? environmentOIDCTokenFilePath;
            environmentCLIProfileDisabled = Environment.GetEnvironmentVariable(Configure.Constants.EnvPrefix + "CLI_PROFILE_DISABLED") ?? environmentCLIProfileDisabled;
            environmentCredentialsURI = Environment.GetEnvironmentVariable(Configure.Constants.EnvPrefix + "CREDENTIALS_URI") ?? environmentCredentialsURI;
            disableECSIMDSv1 = Environment.GetEnvironmentVariable(Configure.Constants.EnvPrefix + "IMDSV1_DISABLED") ?? disableECSIMDSv1;
        }

        public static string GetPrivateKey(string filePath)
        {
            try
            {
                authUtils.privateKey = File.ReadAllText(filePath);
            }
            catch { }

            return authUtils.privateKey;
        }

        public static void SetPrivateKey(string key)
        {
            authUtils.privateKey = key;
        }

        public static string GetOIDCToken(string OIDCTokenFilePath)
        {
            byte[] buffer;
            if (!File.Exists(OIDCTokenFilePath))
            {
                throw new CredentialException("OIDCTokenFilePath " + OIDCTokenFilePath + " does not exist.");
            }
            try
            {
                using (var inStream = new FileStream(OIDCTokenFilePath, FileMode.Open, FileAccess.Read))
                {
                    buffer = new byte[inStream.Length];
                    inStream.Read(buffer, 0, buffer.Length);
                }
                oidcToken = Encoding.UTF8.GetString(buffer);
            }
            catch (UnauthorizedAccessException)
            {
                throw new CredentialException("OIDCTokenFilePath " + OIDCTokenFilePath + " cannot be read.");
            }
            catch (SecurityException)
            {
                throw new CredentialException("Security Exception: Do not have the required permission. " + "OIDCTokenFilePath " + OIDCTokenFilePath);
            }
            catch (IOException e)
            {
                throw new CredentialException(e.Message);
            }
            return oidcToken;
        }

        public static async Task<string> GetOIDCTokenAsync(string OIDCTokenFilePath)
        {
            byte[] buffer;
            if (!File.Exists(OIDCTokenFilePath))
            {
                throw new CredentialException("OIDCTokenFilePath " + OIDCTokenFilePath + " does not exist.");
            }
            try
            {
                using (var inStream = new FileStream(OIDCTokenFilePath, FileMode.Open, FileAccess.Read))
                {
                    buffer = new byte[inStream.Length];
                    await inStream.ReadAsync(buffer, 0, buffer.Length);
                }
                oidcToken = Encoding.UTF8.GetString(buffer);
            }
            catch (UnauthorizedAccessException)
            {
                throw new CredentialException("OIDCTokenFilePath " + OIDCTokenFilePath + " cannot be read.");
            }
            catch (SecurityException)
            {
                throw new CredentialException("Security Exception: Do not have the required permission. " + "OIDCTokenFilePath " + OIDCTokenFilePath);
            }
            catch (IOException e)
            {
                throw new CredentialException(e.Message);
            }
            return oidcToken;
        }

        public static string GetStsRegionWithVpc(string stsRegionId, bool? enableVpc)
        {
            string regionId = string.IsNullOrEmpty(stsRegionId) ? Environment.GetEnvironmentVariable(Configure.Constants.EnvPrefix + "STS_REGION") : stsRegionId;
            var enable = Environment.GetEnvironmentVariable(Configure.Constants.EnvPrefix + "VPC_ENDPOINT_ENABLED") ?? "";
            bool enableVpcEnv = enable.ToLower() == "true";
            string isVpc = (enableVpc == null ? enableVpcEnv : (bool)enableVpc) ? "-vpc" : "";
            if (!string.IsNullOrEmpty(regionId))
            {
                return string.Format("{0}.{1}", isVpc, regionId);
            }
            return "";
        }

        public static string ClientType
        {
            get
            {
                if (string.IsNullOrEmpty(authUtils.clientType))
                {
                    return "default";
                }
                else
                {
                    return authUtils.clientType;
                }
            }
            set { authUtils.clientType = value; }
        }

        public static string EnvironmentAccessKeyId
        {
            get { return authUtils.environmentAccessKeyId; }

            set { authUtils.environmentAccessKeyId = value; }
        }

        public static string EnvironmentAccesskeySecret
        {
            get { return authUtils.environmentAccesskeySecret; }

            set { authUtils.environmentAccesskeySecret = value; }
        }

        public static string EnvironmentSecurityToken
        {
            get { return authUtils.environmentSecurityToken; }

            set { authUtils.environmentSecurityToken = value; }
        }

        public static string EnvironmentEcsMetaData
        {
            get { return authUtils.environmentEcsMetaData; }

            set { authUtils.environmentEcsMetaData = value; }
        }

        public static string EnvironmentEcsMetaDataDisabled
        {
            get { return authUtils.environmentEcsMetaDataDisabled; }

            set { authUtils.environmentEcsMetaDataDisabled = value; }
        }

        public static string EnvironmentCredentialsFile
        {
            get { return authUtils.environmentCredentialsFile; }

            set { authUtils.environmentCredentialsFile = value; }
        }

        public static string EnvironmentOIDCProviderArn
        {
            get { return authUtils.environmentOIDCProviderArn; }

            set { authUtils.environmentOIDCProviderArn = value; }
        }

        public static string EnvironmentOIDCTokenFilePath
        {
            get { return authUtils.environmentOIDCTokenFilePath; }

            set { authUtils.environmentOIDCTokenFilePath = value; }
        }

        public static string EnvironmentRoleArn
        {
            get { return authUtils.environmentRoleArn; }

            set { authUtils.environmentRoleArn = value; }
        }

        public static bool EnvironmentEnableOIDC()
        {
            return !string.IsNullOrEmpty(authUtils.environmentRoleArn)
                && !string.IsNullOrEmpty(authUtils.environmentOIDCProviderArn)
                && !string.IsNullOrEmpty(authUtils.environmentOIDCTokenFilePath);
        }

        public static bool EnvironmentDisableCLIProfile
        {
            get
            {
                return !string.IsNullOrEmpty(authUtils.environmentCLIProfileDisabled)
                    && bool.Parse(authUtils.environmentCLIProfileDisabled);
            }

            set { authUtils.environmentCLIProfileDisabled = value.ToString(); }
        }

        public static string EnvironmentCredentialsURI
        {
            get { return authUtils.environmentCredentialsURI; }

            set { authUtils.environmentCredentialsURI = value; }
        }

        public static bool DisableIMDSv1
        {
            get
            {
                return !string.IsNullOrEmpty(authUtils.disableECSIMDSv1)
                    && bool.Parse(authUtils.disableECSIMDSv1);
            }

            set { authUtils.disableECSIMDSv1 = value.ToString(); }
        }
    }
}
