using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Utils;

namespace Aliyun.Credentials.Provider
{
    /// <summary>
    /// The default credential provider chain of the Credentials tool allows you to use the same code to obtain credentials for different environments based on configurations independent of the application. 
    /// </summary>
    /// <remarks> If you use <c>Client client = new Client()</c> to initialize a Credentials client without specifying an initialization method, the Credentials tool obtains the credential information in the following order:
    /// <list type="number">
    /// <item><description>Obtain the credential information from environment variables</description></item>
    /// <item><description>Obtain the credential information by using the RAM role of an OIDC IdP</description></item>
    /// <item><description>Obtain the credential information from config.json</description></item>
    /// <item><description>Obtain the credential information by using the RAM role of an ECS instance</description></item>
    /// <item><description>Obtain the credential information by URI</description></item>
    /// </list>
    /// </remarks>
    public class DefaultCredentialsProvider : IAlibabaCloudCredentialsProvider
    {
        private readonly List<IAlibabaCloudCredentialsProvider> UserConfigurationProviders =
            new List<IAlibabaCloudCredentialsProvider>();

        private volatile IAlibabaCloudCredentialsProvider lastUsedCredentialsProvider;

        private readonly bool reuseLastProviderEnabled;

        public DefaultCredentialsProvider()
        {
            this.reuseLastProviderEnabled = true;
            CreateDefaultChain();
        }

        public DefaultCredentialsProvider(bool reuseLastProviderEnabled)
        {
            this.reuseLastProviderEnabled = reuseLastProviderEnabled;
            CreateDefaultChain();
        }

        private void CreateDefaultChain()
        {
            UserConfigurationProviders.Add(new EnvironmentVariableCredentialsProvider());
            if (AuthUtils.EnvironmentEnableOIDC())
            {
                UserConfigurationProviders.Add(new OIDCRoleArnCredentialProvider.Builder()
                    .RoleArn(AuthUtils.EnvironmentRoleArn)
                    .OIDCProviderArn(AuthUtils.EnvironmentOIDCProviderArn)
                    .OIDCTokenFilePath(AuthUtils.EnvironmentOIDCTokenFilePath)
                    .Build());
            }
            UserConfigurationProviders.Add(new CLIProfileCredentialsProvider());
            UserConfigurationProviders.Add(new ProfileCredentialsProvider());
            var roleName = AuthUtils.EnvironmentEcsMetaData;
            var metadataDisabled = AuthUtils.EnvironmentEcsMetaDataDisabled ?? "";
            if (metadataDisabled.ToLower() != "true")
            {
                UserConfigurationProviders.Add(new EcsRamRoleCredentialProvider.Builder().RoleName(roleName).Build());
            }
            string uri = AuthUtils.EnvironmentCredentialsURI;
            if (!string.IsNullOrEmpty(uri))
            {
                UserConfigurationProviders.Add(new URLCredentialProvider.Builder().CredentialsURI(uri).Build());
            }
        }

        public CredentialModel GetCredentials()
        {
            if (this.reuseLastProviderEnabled && this.lastUsedCredentialsProvider != null)
            {
                return this.lastUsedCredentialsProvider.GetCredentials();
            }

            CredentialModel credential;
            List<string> errorMessages = new List<string>();
            foreach (IAlibabaCloudCredentialsProvider provider in UserConfigurationProviders)
            {
                try
                {
                    credential = provider.GetCredentials();
                    this.lastUsedCredentialsProvider = provider;
                    if (credential != null)
                    {
                        return new CredentialModel
                        {
                            AccessKeyId = credential.AccessKeyId,
                            AccessKeySecret = credential.AccessKeySecret,
                            SecurityToken = credential.SecurityToken,
                            ProviderName = string.Format("{0}/{1}", this.GetProviderName(), credential.ProviderName)
                        };
                    }
                }
                catch (Exception e)
                {
                    errorMessages.Add(provider.GetType().Name + ": " + e.Message);
                }
            }
            throw new CredentialException("not found credentials: [" + string.Join(", ", errorMessages) + "]");
        }

        public async Task<CredentialModel> GetCredentialsAsync()
        {
            if (this.reuseLastProviderEnabled && this.lastUsedCredentialsProvider != null)
            {
                return await this.lastUsedCredentialsProvider.GetCredentialsAsync();
            }

            CredentialModel credential;
            List<string> errorMessages = new List<string>();
            foreach (IAlibabaCloudCredentialsProvider provider in UserConfigurationProviders)
            {
                try
                {
                    credential = await provider.GetCredentialsAsync();
                    this.lastUsedCredentialsProvider = provider;
                    if (credential != null)
                    {
                        return new CredentialModel
                        {
                            AccessKeyId = credential.AccessKeyId,
                            AccessKeySecret = credential.AccessKeySecret,
                            SecurityToken = credential.SecurityToken,
                            ProviderName = string.Format("{0}/{1}", this.GetProviderName(),  credential.ProviderName)
                        };
                    }
                }
                catch (Exception e)
                {
                    errorMessages.Add(provider.GetType().Name + ": " + e.Message);
                }
            }

            throw new CredentialException("not found credentials: [" + string.Join(", ", errorMessages) + "]");
        }

        public void AddCredentialsProvider(IAlibabaCloudCredentialsProvider provider)
        {
            UserConfigurationProviders.Insert(0, provider);
        }

        public void RemoveCredentialsProvider(IAlibabaCloudCredentialsProvider provider)
        {
            UserConfigurationProviders.Remove(provider);
        }

        public bool ContainsCredentialsProvider(IAlibabaCloudCredentialsProvider provider)
        {
            return UserConfigurationProviders.Contains(provider);
        }

        public void ClearCredentialsProvider()
        {
            UserConfigurationProviders.Clear();
        }

        public string GetProviderName()
        {
            return "default";
        }
    }
}
