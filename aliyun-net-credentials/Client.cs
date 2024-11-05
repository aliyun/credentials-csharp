using System;
using System.Threading.Tasks;
using Aliyun.Credentials.Exceptions;
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

        /// <summary>
        /// param should be instance of <see cref="Config"/> or <see cref="IAlibabaCloudCredentialsProvider"/>
        /// </summary>
        /// <param name="obj"></param>
        /// <exception cref="CredentialException"></exception>
        public Client(object obj)
        {
            if (null == obj)
            {
                credentialsProvider = new DefaultCredentialsProvider();
            }
            else if (obj is Config)
            {
                credentialsProvider = GetProvider((Config)obj);
            } 
            else if (obj is IAlibabaCloudCredentialsProvider)
            {
                credentialsProvider = (IAlibabaCloudCredentialsProvider)obj;
            } 
            else
            {
                throw new CredentialException("Ivalid initialization parameter");
            }
        }

        private IAlibabaCloudCredentialsProvider GetProvider(Config config)
        {
            switch (config.Type)
            {
                case AuthConstant.AccessKey:
                    return new StaticCredentialsProvider(new CredentialModel
                    {
                        AccessKeyId = ParameterHelper.ValidateEnvNotEmpty(config.AccessKeyId, "ALIBABA_CLOUD_ACCESS_KEY_ID", "AccessKeyId", "AccessKeyId must not be null or empty."),
                        AccessKeySecret = ParameterHelper.ValidateEnvNotEmpty(config.AccessKeySecret, "ALIBABA_CLOUD_ACCESS_KEY_SECRET", "AccessKeySecret", "AccessKeySecret must not be null or empty."),
                        Type = AuthConstant.AccessKey,
                    });
                case AuthConstant.Sts:
                    return new StaticCredentialsProvider(new CredentialModel
                    {
                        AccessKeyId = ParameterHelper.ValidateEnvNotEmpty(config.AccessKeyId, "ALIBABA_CLOUD_ACCESS_KEY_ID", "AccessKeyId", "AccessKeyId must not be null or empty."),
                        AccessKeySecret = ParameterHelper.ValidateEnvNotEmpty(config.AccessKeySecret, "ALIBABA_CLOUD_ACCESS_KEY_SECRET", "AccessKeySecret", "AccessKeySecret must not be null or empty."),
                        SecurityToken = ParameterHelper.ValidateEnvNotEmpty(config.SecurityToken, "ALIBABA_CLOUD_SECURITY_TOKEN", "SecurityToken", "SecurityToken must not be null or empty."),
                        Type = AuthConstant.Sts,
                    });
                case AuthConstant.BeareaToken:
                    return new StaticCredentialsProvider(new CredentialModel
                    {
                        BearerToken = ParameterHelper.ValidateNotEmpty(config.BearerToken, "BearerToken", "BearerToken must not be null or empty."),
                        Type = AuthConstant.BeareaToken,
                    });
                case AuthConstant.EcsRamRole:
                    return new EcsRamRoleCredentialProvider.Builder()
                        .RoleName(config.RoleName)
                        .DisableIMDSv1(config.DisableIMDSv1 ?? AuthUtils.DisableIMDSv1)
                        .ConnectTimeout(config.ConnectTimeout)
                        .ReadTimeout(config.Timeout)
                        .Build();
                case AuthConstant.RamRoleArn:
                    IAlibabaCloudCredentialsProvider innerProvider;
                    if (string.IsNullOrEmpty(config.SecurityToken))
                    {
                        innerProvider = new StaticAKCredentialsProvider.Builder()
                            .AccessKeyId(config.AccessKeyId)
                            .AccessKeySecret(config.AccessKeySecret)
                            .Build();
                    }
                    else
                    {
                        innerProvider = new StaticSTSCredentialsProvider.Builder()
                            .AccessKeyId(config.AccessKeyId)
                            .AccessKeySecret(config.AccessKeySecret)
                            .SecurityToken(config.SecurityToken)
                            .Build();
                    }
                    return new RamRoleArnCredentialProvider.Builder()
                        .CredentialsProvider(innerProvider)
                        .DurationSeconds(config.RoleSessionExpiration)
                        .RoleArn(config.RoleArn)
                        .RoleSessionName(config.RoleSessionName)
                        .Policy(config.Policy)
                        .STSEndpoint(config.STSEndpoint)
                        .ExternalId(config.ExternalId)
                        .ConnectTimeout(config.ConnectTimeout)
                        .ReadTimeout(config.Timeout)
                        .Build();
                case AuthConstant.RsaKeyPair:
                    return new RsaKeyPairCredentialProvider(config);
                case AuthConstant.OIDCRoleArn:
                    return new OIDCRoleArnCredentialProvider.Builder()
                        .DurationSeconds(config.RoleSessionExpiration)
                        .RoleArn(config.RoleArn)
                        .RoleSessionName(config.RoleSessionName)
                        .OIDCProviderArn(config.OIDCProviderArn)
                        .OIDCTokenFilePath(config.OIDCTokenFilePath)
                        .Policy(config.Policy)
                        .STSEndpoint(config.STSEndpoint)
                        .ConnectTimeout(config.ConnectTimeout)
                        .ReadTimeout(config.Timeout)
                        .Build();
                case AuthConstant.CredentialsURI:
                    return new URLCredentialProvider.Builder()
                        .CredentialsURI(config.CredentialsURI)
                        .ConnectTimeout(config.ConnectTimeout)
                        .ReadTimeout(config.Timeout)
                        .Build();
                default:
                    throw new CredentialException(string.Format("Unsupported credential type option: {0}, support: access_key, sts, bearer, ecs_ram_role, ram_role_arn, rsa_key_pair, oidc_role_arn, credentials_uri", config.Type));
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
