using System.Collections.Generic;

using Aliyun.Credentials.Exceptions;
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

        public IAlibabaCloudCredentials GetCredentials()
        {
            IAlibabaCloudCredentials credential;
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
