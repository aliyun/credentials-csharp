using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Utils;

namespace Aliyun.Credentials.Provider
{
    public class ProfileCredentialsProvider : IAlibabaCloudCredentialsProvider
    {
        public IAlibabaCloudCredentials GetCredentials()
        {
            string filePath = AuthUtils.EnvironmentCredentialsFile;
            if (filePath == null)
            {
                filePath = AuthConstant.GetDefaultFilePath();
            }
            else if (filePath.Length == 0)
            {
                throw new CredentialException("The specified credentials file is empty");
            }

            IniFileHelper iniFile;
            try
            {
                iniFile = new IniFileHelper(filePath);
            }
            catch (IOException)
            {
                return null;
            }

            if (!iniFile.Ini.ContainsKey(AuthUtils.ClientType))
            {
                throw new CredentialException("Client is not open in the specified credentials file");
            }
            return CreateCredential(iniFile.Ini[AuthUtils.ClientType]);
        }

        public async Task<IAlibabaCloudCredentials> GetCredentialsAsync()
        {
            string filePath = AuthUtils.EnvironmentCredentialsFile;
            if (filePath == null)
            {
                filePath = AuthConstant.GetDefaultFilePath();
            }
            else if (filePath.Length == 0)
            {
                throw new CredentialException("The specified credentials file is empty");
            }

            IniFileHelper iniFile;
            try
            {
                iniFile = new IniFileHelper(filePath);
            }
            catch (IOException)
            {
                return null;
            }

            if (!iniFile.Ini.ContainsKey(AuthUtils.ClientType))
            {
                throw new CredentialException("Client is not open in the specified credentials file");
            }
            return await CreateCredentialAsync(iniFile.Ini[AuthUtils.ClientType]);
        }

        private IAlibabaCloudCredentials CreateCredential(Dictionary<string, string> clientConfig)
        {

            string configType = clientConfig[AuthConstant.IniType];
            if (string.IsNullOrWhiteSpace(configType))
            {
                throw new CredentialException("The configured client type is empty");
            }

            switch (configType)
            {
                case AuthConstant.IniTypeArn:
                    return GetSTSAssumeRoleSessionCredentials(clientConfig);
                case AuthConstant.IniTypeKeyPair:
                    return GetSTSGetSessionAccessKeyCredentials(clientConfig);
                case AuthConstant.IniTypeRam:
                    return GetInstanceProfileCredentials(clientConfig);
            }

            string accessKeyId = clientConfig[AuthConstant.IniAccessKeyId];
            string accessKeySecret = clientConfig[AuthConstant.IniAccessKeyIdsecret];
            if (string.IsNullOrWhiteSpace(accessKeyId) || string.IsNullOrWhiteSpace(accessKeySecret))
            {
                return null;
            }

            return new AccessKeyCredential(accessKeyId, accessKeySecret);
        }

        private async Task<IAlibabaCloudCredentials> CreateCredentialAsync(Dictionary<string, string> clientConfig)
        {

            string configType = clientConfig[AuthConstant.IniType];
            if (string.IsNullOrWhiteSpace(configType))
            {
                throw new CredentialException("The configured client type is empty");
            }

            switch (configType)
            {
                case AuthConstant.IniTypeArn:
                    return await GetSTSAssumeRoleSessionCredentialsAsync(clientConfig);
                case AuthConstant.IniTypeKeyPair:
                    return await GetSTSGetSessionAccessKeyCredentialsAsync(clientConfig);
                case AuthConstant.IniTypeRam:
                    return await GetInstanceProfileCredentialsAsync(clientConfig);
            }

            string accessKeyId = clientConfig[AuthConstant.IniAccessKeyId];
            string accessKeySecret = clientConfig[AuthConstant.IniAccessKeyIdsecret];
            if (string.IsNullOrWhiteSpace(accessKeyId) || string.IsNullOrWhiteSpace(accessKeySecret))
            {
                return null;
            }

            return new AccessKeyCredential(accessKeyId, accessKeySecret);
        }

        public IAlibabaCloudCredentials GetSTSAssumeRoleSessionCredentials(Dictionary<string, string> clientConfig)
        {
            string accessKeyId = DictionaryUtil.Get(clientConfig, AuthConstant.IniAccessKeyId);
            string accessKeySecret = DictionaryUtil.Get(clientConfig, AuthConstant.IniAccessKeyIdsecret);
            string roleSessionName = DictionaryUtil.Get(clientConfig, AuthConstant.IniRoleSessionName);
            string roleArn = DictionaryUtil.Get(clientConfig, AuthConstant.IniRoleArn);
            string regionId = DictionaryUtil.Get(clientConfig, AuthConstant.DefaultRegion);
            string policy = DictionaryUtil.Get(clientConfig, AuthConstant.IniPolicy);

            if (string.IsNullOrWhiteSpace(accessKeyId) || string.IsNullOrWhiteSpace(accessKeySecret))
            {
                throw new CredentialException("The configured access_key_id or access_key_secret is empty");
            }

            if (string.IsNullOrWhiteSpace(roleSessionName) || string.IsNullOrWhiteSpace(roleArn))
            {
                throw new CredentialException("The configured role_session_name or role_arn is empty");
            }

            RamRoleArnCredentialProvider provider = new RamRoleArnCredentialProvider(accessKeyId,
                accessKeySecret, roleSessionName, roleArn, regionId, policy);
            return provider.GetCredentials();
        }

        public async Task<IAlibabaCloudCredentials> GetSTSAssumeRoleSessionCredentialsAsync(Dictionary<string, string> clientConfig)
        {
            string accessKeyId = DictionaryUtil.Get(clientConfig, AuthConstant.IniAccessKeyId);
            string accessKeySecret = DictionaryUtil.Get(clientConfig, AuthConstant.IniAccessKeyIdsecret);
            string roleSessionName = DictionaryUtil.Get(clientConfig, AuthConstant.IniRoleSessionName);
            string roleArn = DictionaryUtil.Get(clientConfig, AuthConstant.IniRoleArn);
            string regionId = DictionaryUtil.Get(clientConfig, AuthConstant.DefaultRegion);
            string policy = DictionaryUtil.Get(clientConfig, AuthConstant.IniPolicy);

            if (string.IsNullOrWhiteSpace(accessKeyId) || string.IsNullOrWhiteSpace(accessKeySecret))
            {
                throw new CredentialException("The configured access_key_id or access_key_secret is empty");
            }

            if (string.IsNullOrWhiteSpace(roleSessionName) || string.IsNullOrWhiteSpace(roleArn))
            {
                throw new CredentialException("The configured role_session_name or role_arn is empty");
            }

            RamRoleArnCredentialProvider provider = new RamRoleArnCredentialProvider(accessKeyId,
                accessKeySecret, roleSessionName, roleArn, regionId, policy);
            return await provider.GetCredentialsAsync();
        }

        public IAlibabaCloudCredentials GetSTSGetSessionAccessKeyCredentials(Dictionary<string, string> clientConfig)
        {
            string publicKeyId = DictionaryUtil.Get(clientConfig, AuthConstant.IniPublicKeyId);
            string privateKeyFile = DictionaryUtil.Get(clientConfig, AuthConstant.IniPrivateKeyFile);
            if (string.IsNullOrWhiteSpace(privateKeyFile))
            {
                throw new CredentialException("The configured private_key_file is empty");
            }

            string privateKey = AuthUtils.GetPrivateKey(privateKeyFile);
            if (string.IsNullOrWhiteSpace(publicKeyId) || string.IsNullOrWhiteSpace(privateKey))
            {
                throw new CredentialException("The configured public_key_id or private_key_file content is empty");
            }

            RsaKeyPairCredentialProvider provider = new RsaKeyPairCredentialProvider(publicKeyId, privateKey);
            return provider.GetCredentials();
        }

        public async Task<IAlibabaCloudCredentials> GetSTSGetSessionAccessKeyCredentialsAsync(Dictionary<string, string> clientConfig)
        {
            string publicKeyId = DictionaryUtil.Get(clientConfig, AuthConstant.IniPublicKeyId);
            string privateKeyFile = DictionaryUtil.Get(clientConfig, AuthConstant.IniPrivateKeyFile);
            if (string.IsNullOrWhiteSpace(privateKeyFile))
            {
                throw new CredentialException("The configured private_key_file is empty");
            }

            string privateKey = AuthUtils.GetPrivateKey(privateKeyFile);
            if (string.IsNullOrWhiteSpace(publicKeyId) || string.IsNullOrWhiteSpace(privateKey))
            {
                throw new CredentialException("The configured public_key_id or private_key_file content is empty");
            }

            RsaKeyPairCredentialProvider provider = new RsaKeyPairCredentialProvider(publicKeyId, privateKey);
            return await provider.GetCredentialsAsync();
        }

        public IAlibabaCloudCredentials GetInstanceProfileCredentials(Dictionary<string, string> clientConfig)
        {
            string roleName = DictionaryUtil.Get(clientConfig, AuthConstant.IniRoleName);
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new CredentialException("The configured role_name is empty");
            }

            EcsRamRoleCredentialProvider provider = new EcsRamRoleCredentialProvider(roleName);
            return provider.GetCredentials();
        }

        public async Task<IAlibabaCloudCredentials> GetInstanceProfileCredentialsAsync(Dictionary<string, string> clientConfig)
        {
            string roleName = DictionaryUtil.Get(clientConfig, AuthConstant.IniRoleName);
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new CredentialException("The configured role_name is empty");
            }

            EcsRamRoleCredentialProvider provider = new EcsRamRoleCredentialProvider(roleName);
            return await provider.GetCredentialsAsync();
        }
    }
}
