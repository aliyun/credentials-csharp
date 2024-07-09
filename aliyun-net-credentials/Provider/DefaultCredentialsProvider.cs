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

        public DefaultCredentialsProvider()
        {
            UserConfigurationProviders.Add(new EnvironmentVariableCredentialsProvider());
            UserConfigurationProviders.Add(new ProfileCredentialsProvider());
            var roleName = AuthUtils.EnvironmentEcsMetaData;

            UserConfigurationProviders.Add(new EcsRamRoleCredentialProvider(roleName));
        }

        public CredentialModel GetCredentials()
        {
            CredentialModel credential;
            foreach (IAlibabaCloudCredentialsProvider provider in UserConfigurationProviders)
            {
                credential = provider.GetCredentials();
                if (credential != null)
                {
                    return credential;
                }
            }

            throw new CredentialException("not found credentials");
        }

        public async Task<CredentialModel> GetCredentialsAsync()
        {
            CredentialModel credential;
            foreach (IAlibabaCloudCredentialsProvider provider in UserConfigurationProviders)
            {
                credential = await provider.GetCredentialsAsync();
                if (credential != null)
                {
                    return credential;
                }
            }

            throw new CredentialException("not found credentials");
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
