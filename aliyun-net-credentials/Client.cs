using System;
using System.Threading.Tasks;

using Aliyun.Credentials.Models;
using Aliyun.Credentials.Provider;
using Aliyun.Credentials.Utils;

namespace Aliyun.Credentials
{
    public class Client
    {
        private readonly IAlibabaCloudCredentialsProvider credentialsProvider;

        public Client()
        {
            credentialsProvider = new DefaultCredentialsProvider();
        }

        public Client(Config config)
        {
            if (null == config)
            {
                credentialsProvider = new DefaultCredentialsProvider();
            }
            else
            {
                credentialsProvider = GetProvider(config);
            }
        }

        private IAlibabaCloudCredentialsProvider GetProvider(Config config)
        {
            switch (config.Type)
            {
                case AuthConstant.AccessKey:
                    return new StaticCredentialsProvider(new CredentialModel
                    {
                        AccessKeyId = ParameterHelper.ValidateEnvNotNull(config.AccessKeyId, "ALIBABA_CLOUD_ACCESS_KEY_ID",  "AccessKeyId", "AccessKeyId must not be null."),
                        AccessKeySecret = ParameterHelper.ValidateEnvNotNull(config.AccessKeySecret, "ALIBABA_CLOUD_ACCESS_KEY_SECRET", "AccessKeySecret", "AccessKeySecret must not be null."),
                        Type = AuthConstant.AccessKey,
                    });
                case AuthConstant.Sts:
                    return new StaticCredentialsProvider(new CredentialModel
                    {
                        AccessKeyId = ParameterHelper.ValidateEnvNotNull(config.AccessKeyId, "ALIBABA_CLOUD_ACCESS_KEY_ID",  "AccessKeyId", "AccessKeyId must not be null."),
                        AccessKeySecret = ParameterHelper.ValidateEnvNotNull(config.AccessKeySecret, "ALIBABA_CLOUD_ACCESS_KEY_SECRET", "AccessKeySecret", "AccessKeySecret must not be null."),
                        SecurityToken = ParameterHelper.ValidateEnvNotNull(config.SecurityToken, "ALIBABA_CLOUD_SECURITY_TOKEN", "SecurityToken", "SecurityToken must not be null."),
                        Type = AuthConstant.Sts,
                    });
                case AuthConstant.BeareaToken:
                    return new StaticCredentialsProvider(new CredentialModel
                    {
                        BearerToken = ParameterHelper.ValidateNotNull(config.BearerToken, "BearerToken", "BearerToken must not be null."),
                        Type = AuthConstant.BeareaToken,
                    });
                case AuthConstant.EcsRamRole:
                    return new EcsRamRoleCredentialProvider(config);
                case AuthConstant.RamRoleArn:
                    return new RamRoleArnCredentialProvider(config);
                case AuthConstant.RsaKeyPair:
                    return new RsaKeyPairCredentialProvider(config);
                case AuthConstant.OIDCRoleArn:
                    return new OIDCRoleArnCredentialProvider(config);
                case AuthConstant.URLSts:
                    return new URLCredentialProvider(config);
                default:
                    return new DefaultCredentialsProvider();
            }
        }

        public CredentialModel GetCredential()
        {
            return credentialsProvider.GetCredentials();
        }

        public async Task<CredentialModel> GetCredentialAsync()
        {
            return await credentialsProvider.GetCredentialsAsync();
        }

        [Obsolete("Use GetCredential().AccessKeyId instead.")]
        public string GetAccessKeyId()
        {
            return GetCredential().AccessKeyId;
        }

        [Obsolete("Get AccessKeyId from GetCredentialAsync() instead.")]
        public async Task<string> GetAccessKeyIdAsync()
        {
            var credential = await GetCredentialAsync();
            return credential.AccessKeyId;
        }

        [Obsolete("Use GetCredential().AccessKeySecret instead.")]
        public string GetAccessKeySecret()
        {
            return GetCredential().AccessKeySecret;
        }

        [Obsolete("Get AccessKeySecret from GetCredentialAsync() instead.")]
        public async Task<string> GetAccessKeySecretAsync()
        {
            var credential = await GetCredentialAsync();
            return credential.AccessKeySecret;
        }

        [Obsolete("Use GetCredential().SecurityToken instead.")]
        public string GetSecurityToken()
        {
            return GetCredential().SecurityToken;
        }

        [Obsolete("Get SecurityToken from GetCredentialAsync() instead.")]
        public async Task<string> GetSecurityTokenAsync()
        {
            var credential = await GetCredentialAsync();
            return credential.SecurityToken;
        }

        [Obsolete("Use GetCredential().BearerToken instead.")]
        public string GetBearerToken()
        {
            return GetCredential().BearerToken;
        }

        [Obsolete("Get BearerToken from GetCredentialAsync() instead.")]
        public async Task<string> GetBearerTokenAsync()
        {
            var credential = await GetCredentialAsync();
            return credential.BearerToken;
        }

        [Obsolete("Use GetCredential().Type instead.")]
        public new string GetType()
        {
            return GetCredential().Type;
        }

        [Obsolete("Get Type from GetCredentialAsync() instead.")]
        public async Task<string> GetTypeAsync()
        {
            var credential = await GetCredentialAsync();
            return credential.Type;
        }
    }
}
