using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Utils;

namespace Aliyun.Credentials.Provider
{
    /// <summary>
    /// Obtain the credential information from a configuration file.
    /// <list type="bullet">
    /// <item><description>Linux: ~/.alibabacloud/credentials</description></item>
    /// <item><description>Windows: C:\Users\USER_NAME\.alibabacloud\credentials</description></item>
    /// </list>
    /// </summary>
    public class ProfileCredentialsProvider : IAlibabaCloudCredentialsProvider
    {
        public CredentialModel GetCredentials()
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
                throw new CredentialException(string.Format("Unable to open credentials file: {0}.", filePath));
            }

            if (!iniFile.Ini.ContainsKey(AuthUtils.ClientType))
            {
                throw new CredentialException("Client is not open in the specified credentials file");
            }
            return CreateCredential(iniFile.Ini[AuthUtils.ClientType]);
        }

        public async Task<CredentialModel> GetCredentialsAsync()
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
                throw new CredentialException(string.Format("Unable to open credentials file: {0}.", filePath));
            }

            if (!iniFile.Ini.ContainsKey(AuthUtils.ClientType))
            {
                throw new CredentialException("Client is not open in the specified credentials file");
            }

            CredentialModel credentialModel = await CreateCredentialAsync(iniFile.Ini[AuthUtils.ClientType]);
            credentialModel.ProviderName = string.Format("{0}/{1}", this.GetProviderName(), credentialModel.ProviderName);
            return credentialModel;
        }

        private CredentialModel CreateCredential(Dictionary<string, string> clientConfig)
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
                case AuthConstant.OIDCRoleArn:
                    return GetSTSOIDCRoleSessionCredentials(clientConfig);
            }

            string accessKeyId = clientConfig[AuthConstant.IniAccessKeyId];
            string accessKeySecret = clientConfig[AuthConstant.IniAccessKeyIdsecret];
            if (string.IsNullOrWhiteSpace(accessKeyId) || string.IsNullOrWhiteSpace(accessKeySecret))
            {
                throw new CredentialException("The configured access_key_id or access_key_secret is empty");
            }

            return new CredentialModel
            {
                AccessKeyId = accessKeyId,
                AccessKeySecret = accessKeySecret,
                Type = AuthConstant.AccessKey,
                ProviderName = "static_ak"
            };
        }

        private async Task<CredentialModel> CreateCredentialAsync(Dictionary<string, string> clientConfig)
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
                case AuthConstant.OIDCRoleArn:
                    return await GetSTSOIDCRoleSessionCredentialsAsync(clientConfig);
            }

            string accessKeyId = clientConfig[AuthConstant.IniAccessKeyId];
            string accessKeySecret = clientConfig[AuthConstant.IniAccessKeyIdsecret];
            if (string.IsNullOrWhiteSpace(accessKeyId) || string.IsNullOrWhiteSpace(accessKeySecret))
            {
                throw new CredentialException("The configured access_key_id or access_key_secret is empty");
            }

            return new CredentialModel
            {
                AccessKeyId = accessKeyId,
                AccessKeySecret = accessKeySecret,
                Type = AuthConstant.AccessKey,
                ProviderName = "static_ak"
            };
        }

        public CredentialModel GetSTSAssumeRoleSessionCredentials(Dictionary<string, string> clientConfig)
        {
            string accessKeyId = DictionaryUtil.Get(clientConfig, AuthConstant.IniAccessKeyId);
            string accessKeySecret = DictionaryUtil.Get(clientConfig, AuthConstant.IniAccessKeyIdsecret);
            string roleSessionName = DictionaryUtil.Get(clientConfig, AuthConstant.IniRoleSessionName);
            string roleArn = DictionaryUtil.Get(clientConfig, AuthConstant.IniRoleArn);
            string regionId = DictionaryUtil.Get(clientConfig, AuthConstant.DefaultRegion);
            string policy = DictionaryUtil.Get(clientConfig, AuthConstant.IniPolicy);
            string stsRegionId = DictionaryUtil.Get(clientConfig, AuthConstant.iniStsRegionId);
            string externalId = DictionaryUtil.Get(clientConfig, AuthConstant.iniExternalId);
            string enable = DictionaryUtil.Get(clientConfig, AuthConstant.IniEnable);
            bool? enableVpc = enable == null ? (bool?)null
                : enable.ToLower() == "true" ? true
                : enable.ToLower() == "false" ? false
                : (bool?)null;

            if (string.IsNullOrWhiteSpace(accessKeyId) || string.IsNullOrWhiteSpace(accessKeySecret))
            {
                throw new CredentialException("The configured access_key_id or access_key_secret is empty");
            }

            if (string.IsNullOrWhiteSpace(roleSessionName) || string.IsNullOrWhiteSpace(roleArn))
            {
                throw new CredentialException("The configured role_session_name or role_arn is empty");
            }

            RamRoleArnCredentialProvider provider = new RamRoleArnCredentialProvider.Builder()
                .AccessKeyId(accessKeyId)
                .AccessKeySecret(accessKeySecret)
                .RoleSessionName(roleSessionName)
                .RoleArn(roleArn)
                .RegionId(regionId)
                .Policy(policy)
                .StsRegionId(stsRegionId)
                .EnableVpc(enableVpc)
                .ExternalId(externalId)
                .Build();
            return provider.GetCredentials();
        }

        public async Task<CredentialModel> GetSTSAssumeRoleSessionCredentialsAsync(Dictionary<string, string> clientConfig)
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

            RamRoleArnCredentialProvider provider = new RamRoleArnCredentialProvider.Builder()
                .AccessKeyId(accessKeyId)
                .AccessKeySecret(accessKeySecret)
                .RoleSessionName(roleSessionName)
                .RoleArn(roleArn)
                .RegionId(regionId)
                .Policy(policy)
                .Build();
            return await provider.GetCredentialsAsync();
        }

        public CredentialModel GetSTSGetSessionAccessKeyCredentials(Dictionary<string, string> clientConfig)
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

        public async Task<CredentialModel> GetSTSGetSessionAccessKeyCredentialsAsync(Dictionary<string, string> clientConfig)
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

        public CredentialModel GetInstanceProfileCredentials(Dictionary<string, string> clientConfig)
        {
            string roleName = DictionaryUtil.Get(clientConfig, AuthConstant.IniRoleName);
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new CredentialException("The configured role_name is empty");
            }

            EcsRamRoleCredentialProvider provider = new EcsRamRoleCredentialProvider.Builder().RoleName(roleName).Build();
            return provider.GetCredentials();
        }

        public async Task<CredentialModel> GetInstanceProfileCredentialsAsync(Dictionary<string, string> clientConfig)
        {
            string roleName = DictionaryUtil.Get(clientConfig, AuthConstant.IniRoleName);
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new CredentialException("The configured role_name is empty");
            }

            EcsRamRoleCredentialProvider provider = new EcsRamRoleCredentialProvider.Builder().RoleName(roleName).Build();
            return await provider.GetCredentialsAsync();
        }

        public CredentialModel GetSTSOIDCRoleSessionCredentials(Dictionary<string, string> clientConfig)
        {
            string roleSessionName = DictionaryUtil.Get(clientConfig, AuthConstant.IniRoleSessionName);
            string roleArn = DictionaryUtil.Get(clientConfig, AuthConstant.IniRoleArn);
            string OIDCProviderArn = DictionaryUtil.Get(clientConfig, AuthConstant.IniOIDCProviderArn);
            string OIDCTokenFilePath = DictionaryUtil.Get(clientConfig, AuthConstant.IniOIDCTokenFilePath);
            string regionId = DictionaryUtil.Get(clientConfig, AuthConstant.DefaultRegion);
            string policy = DictionaryUtil.Get(clientConfig, AuthConstant.IniPolicy);

            if (string.IsNullOrWhiteSpace(roleArn))
            {
                throw new CredentialException("The configured role_arn is empty");
            }

            if (string.IsNullOrWhiteSpace(OIDCProviderArn))
            {
                throw new CredentialException("The configured oidc_provider_arn is empty");
            }

            OIDCRoleArnCredentialProvider provider = new OIDCRoleArnCredentialProvider.Builder()
                .RoleArn(roleArn)
                .OIDCProviderArn(OIDCProviderArn)
                .OIDCTokenFilePath(OIDCTokenFilePath)
                .RoleSessionName(roleSessionName)
                .RegionId(regionId)
                .Policy(policy)
                .Build();
            return provider.GetCredentials();
        }

        public async Task<CredentialModel> GetSTSOIDCRoleSessionCredentialsAsync(Dictionary<string, string> clientConfig)
        {
            string roleSessionName = DictionaryUtil.Get(clientConfig, AuthConstant.IniRoleSessionName);
            string roleArn = DictionaryUtil.Get(clientConfig, AuthConstant.IniRoleArn);
            string OIDCProviderArn = DictionaryUtil.Get(clientConfig, AuthConstant.IniOIDCProviderArn);
            string OIDCTokenFilePath = DictionaryUtil.Get(clientConfig, AuthConstant.IniOIDCTokenFilePath);
            string regionId = DictionaryUtil.Get(clientConfig, AuthConstant.DefaultRegion);
            string policy = DictionaryUtil.Get(clientConfig, AuthConstant.IniPolicy);
            string stsRegionId = DictionaryUtil.Get(clientConfig, AuthConstant.iniStsRegionId);
            string enable = DictionaryUtil.Get(clientConfig, AuthConstant.IniEnable);
            bool? enableVpc = enable == null ? (bool?)null
                : enable.ToLower() == "true" ? true
                : enable.ToLower() == "false" ? false
                : (bool?)null;

            if (string.IsNullOrWhiteSpace(roleArn))
            {
                throw new CredentialException("The configured role_arn is empty");
            }

            if (string.IsNullOrWhiteSpace(OIDCProviderArn))
            {
                throw new CredentialException("The configured oidc_provider_arn is empty");
            }

            OIDCRoleArnCredentialProvider provider = new OIDCRoleArnCredentialProvider.Builder()
                .RoleArn(roleArn)
                .OIDCProviderArn(OIDCProviderArn)
                .OIDCTokenFilePath(OIDCTokenFilePath)
                .RoleSessionName(roleSessionName)
                .RegionId(regionId)
                .Policy(policy)
                .StsRegionId(stsRegionId)
                .EnableVpc(enableVpc)
                .Build();
            return await provider.GetCredentialsAsync();
        }

        public string GetProviderName()
        {
            return "profile";
        }

    }
}
