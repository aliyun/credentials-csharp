using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Utils;

namespace Aliyun.Credentials.Provider
{
    public class DefaultCredentialsProvider : IAlibabaCloudCredentialsProvider
    {
        private readonly List<IAlibabaCloudCredentialsProvider> UserConfigurationProviders =
            new List<IAlibabaCloudCredentialsProvider>();

        private volatile IAlibabaCloudCredentialsProvider lastUsedCredentialsProvider;

        private readonly bool reuseLastProviderEnabled;

        public DefaultCredentialsProvider()
        {
            this.reuseLastProviderEnabled = true;
            createDefaultChain();
        }

        public DefaultCredentialsProvider(bool reuseLastProviderEnabled)
        {
            this.reuseLastProviderEnabled = reuseLastProviderEnabled;
            createDefaultChain();
        }

        private void createDefaultChain()
        {
            UserConfigurationProviders.Add(new EnvironmentVariableCredentialsProvider());
            if (AuthUtils.EnvironmentEnableOIDC())
            {
                Config config = new Config
                {
                    RoleArn = AuthUtils.EnvironmentRoleArn,
                    OIDCProviderArn = AuthUtils.EnvironmentOIDCProviderArn,
                    OIDCTokenFilePath = AuthUtils.EnvironmentOIDCTokenFilePath
                };
                UserConfigurationProviders.Add(new OIDCRoleArnCredentialProvider(config));
            }
            UserConfigurationProviders.Add(new CLIProfileCredentialsProvider());
            UserConfigurationProviders.Add(new ProfileCredentialsProvider());
            var roleName = AuthUtils.EnvironmentEcsMetaData;
            if (null != roleName)
            {
                UserConfigurationProviders.Add(new EcsRamRoleCredentialProvider(roleName));
            }
            string uri = AuthUtils.EnvironmentCredentialsURI;
            if (!string.IsNullOrEmpty(uri))
            {
                UserConfigurationProviders.Add(new URLCredentialProvider(uri));
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
                        return credential;
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
                        return credential;
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
    }
}
