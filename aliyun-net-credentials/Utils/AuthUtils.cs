using System;
using System.IO;

namespace Aliyun.Credentials.Utils
{
    public class AuthUtils
    {
        private string clientType;
        private string environmentAccessKeyId;
        private string environmentAccesskeySecret;
        private string environmentEcsMetaData;
        private string environmentCredentialsFile;
        private string privateKey;

        static AuthUtils authUtils = new AuthUtils();

        AuthUtils()
        {
            clientType = Environment.GetEnvironmentVariable("ALIBABA_CLOUD_PROFILE");
            environmentAccessKeyId = Environment.GetEnvironmentVariable("ALIBABA_CLOUD_ACCESS_KEY_ID");
            environmentAccesskeySecret = Environment.GetEnvironmentVariable("ALIBABA_CLOUD_ACCESS_KEY_SECRET");
            environmentEcsMetaData = Environment.GetEnvironmentVariable("ALIBABA_CLOUD_ECS_METADATA");
            environmentCredentialsFile = Environment.GetEnvironmentVariable("ALIBABA_CLOUD_CREDENTIALS_FILE");
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

        public static string ClientType
        {
            get
            {
                if (String.IsNullOrEmpty(authUtils.clientType))
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

        public static string EnvironmentEcsMetaData
        {
            get { return authUtils.environmentEcsMetaData; }

            set { authUtils.environmentEcsMetaData = value; }
        }

        public static string EnvironmentCredentialsFile
        {
            get { return authUtils.environmentCredentialsFile; }

            set { authUtils.environmentCredentialsFile = value; }
        }
    }
}
