using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Utils;

using Newtonsoft.Json;

namespace Aliyun.Credentials.Provider
{
    /// <summary>
    /// Obtain the credential information from a configuration file. The path of the configuration file varies based on the operating system:
    /// <list type="bullet">
    /// <item><description>Linux: ~/.aliyun/config.json</description></item>
    /// <item><description>Windows: C:\Users\USER_NAME\.aliyun\config.json</description></item>
    /// </list>
    /// </summary>
    public class CLIProfileCredentialsProvider : IAlibabaCloudCredentialsProvider
    {
        private readonly string CLI_CREDENTIALS_CONFIG_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".aliyun", "config.json");
        private volatile IAlibabaCloudCredentialsProvider credentialsProvider;
        private volatile string currentProfileName;
        private readonly object credentialsProviderLock = new object();

        public CLIProfileCredentialsProvider(string profileName = null)
        {
            currentProfileName = profileName ?? Environment.GetEnvironmentVariable("ALIBABA_CLOUD_PROFILE");
        }

        public CredentialModel GetCredentials()
        {
            if (AuthUtils.EnvironmentDisableCLIProfile)
            {
                throw new CredentialException("CLI credentials file is disabled.");
            }
            Config config = ParseProfile(CLI_CREDENTIALS_CONFIG_PATH);
            if (config == null)
            {
                throw new CredentialException("Unable to get profile form empty CLI credentials file.");
            }
            string refreshedProfileName = Environment.GetEnvironmentVariable("ALIBABA_CLOUD_PROFILE");

            if (ShouldReloadCredentialsProvider(refreshedProfileName))
            {
                lock (credentialsProviderLock)
                {
                    if (ShouldReloadCredentialsProvider(refreshedProfileName))
                    {
                        if (!string.IsNullOrEmpty(refreshedProfileName))
                        {
                            this.currentProfileName = refreshedProfileName;
                        }
                        this.credentialsProvider = ReloadCredentialsProvider(config, this.currentProfileName);
                    }
                }
            }
            var credentials = this.credentialsProvider.GetCredentials();
            return new CredentialModel
            {
                AccessKeyId = credentials.AccessKeyId,
                AccessKeySecret = credentials.AccessKeySecret,
                SecurityToken = credentials.SecurityToken,
                ProviderName = string.Format("{0}/{1}", this.GetProviderName(), credentials.ProviderName)
            };
        }

        internal CredentialModel GetCredentials(string path)
        {
            if (AuthUtils.EnvironmentDisableCLIProfile)
            {
                throw new CredentialException("CLI credentials file is disabled.");
            }
            Config config = ParseProfile(path);
            if (config == null)
            {
                throw new CredentialException("Unable to get profile form empty CLI credentials file.");
            }
            string refreshedProfileName = Environment.GetEnvironmentVariable("ALIBABA_CLOUD_PROFILE");

            if (ShouldReloadCredentialsProvider(refreshedProfileName))
            {
                lock (credentialsProviderLock)
                {
                    if (ShouldReloadCredentialsProvider(refreshedProfileName))
                    {
                        if (!string.IsNullOrEmpty(refreshedProfileName))
                        {
                            this.currentProfileName = refreshedProfileName;
                        }
                        this.credentialsProvider = ReloadCredentialsProvider(config, this.currentProfileName);
                    }
                }
            }
            var credentials = this.credentialsProvider.GetCredentials();
            return new CredentialModel
            {
                AccessKeyId = credentials.AccessKeyId,
                AccessKeySecret = credentials.AccessKeySecret,
                SecurityToken = credentials.SecurityToken,
                ProviderName = string.Format("{0}/{1}", this.GetProviderName(), credentials.ProviderName)
            };
        }

        public async Task<CredentialModel> GetCredentialsAsync()
        {
            if (AuthUtils.EnvironmentDisableCLIProfile)
            {
                throw new CredentialException("CLI credentials file is disabled.");
            }
            Config config = ParseProfile(CLI_CREDENTIALS_CONFIG_PATH);
            if (config == null)
            {
                throw new CredentialException("Unable to get profile form empty CLI credentials file.");
            }
            string refreshedProfileName = Environment.GetEnvironmentVariable("ALIBABA_CLOUD_PROFILE");

            if (ShouldReloadCredentialsProvider(refreshedProfileName))
            {
                lock (credentialsProviderLock)
                {
                    if (ShouldReloadCredentialsProvider(refreshedProfileName))
                    {
                        if (!string.IsNullOrEmpty(refreshedProfileName))
                        {
                            this.currentProfileName = refreshedProfileName;
                        }
                        this.credentialsProvider = ReloadCredentialsProvider(config, this.currentProfileName);
                    }
                }
            }
            var credentials = await this.credentialsProvider.GetCredentialsAsync();
            return new CredentialModel
            {
                AccessKeyId = credentials.AccessKeyId,
                AccessKeySecret = credentials.AccessKeySecret,
                SecurityToken = credentials.SecurityToken,
                ProviderName = string.Format("{0}/{1}", this.GetProviderName(), credentials.ProviderName)
            };
        }

        internal string GetProfileName()
        {
            return this.currentProfileName;
        }

        internal IAlibabaCloudCredentialsProvider ReloadCredentialsProvider(Config config, string profileName)
        {
            string currentProfileName = !string.IsNullOrEmpty(profileName) ? profileName : config.GetCurrent();
            List<Profile> profiles = config.GetProfiles();
            if (profiles != null && profiles.Count > 0)
            {
                foreach (var profile in profiles)
                {
                    if (!string.IsNullOrEmpty(profile.GetName()) && profile.GetName().Equals(currentProfileName))
                    {
                        switch (profile.GetMode())
                        {
                            case "AK":
                                CredentialModel credentialModel = new CredentialModel
                                {
                                    AccessKeyId = ParameterHelper.ValidateNotEmpty(profile.GetAccessKeyId(), "AccessKeyId", "AccessKeyId must not be null or emptry."),
                                    AccessKeySecret = ParameterHelper.ValidateNotEmpty(profile.GetAccessKeySecret(), "AccessKeySecret", "AccessKeySecret must not be null or empty."),
                                    Type = AuthConstant.AccessKey
                                };
                                Models.Config credentialsConfig = new Models.Config
                                {
                                    AccessKeyId = profile.GetAccessKeyId(),
                                    AccessKeySecret = profile.GetAccessKeySecret()
                                };
                                return new StaticAKCredentialsProvider(credentialsConfig);
                            case "RamRoleArn":
                                credentialModel = new CredentialModel
                                {
                                    AccessKeyId = ParameterHelper.ValidateNotEmpty(profile.GetAccessKeyId(), "AccessKeyId", "AccessKeyId must not be null or empty."),
                                    AccessKeySecret = ParameterHelper.ValidateNotEmpty(profile.GetAccessKeySecret(), "AccessKeySecret", "AccessKeySecret must not be null or empty."),
                                    Type = AuthConstant.AccessKey
                                };
                                IAlibabaCloudCredentialsProvider innerProvider = new StaticCredentialsProvider(credentialModel);
                                return new RamRoleArnCredentialProvider.Builder()
                                    .CredentialsProvider(innerProvider)
                                    .RoleArn(profile.GetRoleArn())
                                    .DurationSeconds(profile.GetDurationSeconds())
                                    .RoleSessionName(profile.GetRoleSessionName())
                                    .StsRegionId(profile.GetStsRegionId())
                                    .EnableVpc(profile.GetEnableVpc())
                                    .Policy(profile.GetPolicy())
                                    .ExternalId(profile.GetExternalId())
                                    .Build();
                            case "EcsRamRole":
                                return new EcsRamRoleCredentialProvider.Builder().RoleName(profile.GetRamRoleName()).Build();
                            case "OIDC":
                                return new OIDCRoleArnCredentialProvider.Builder()
                                    .DurationSeconds(profile.GetDurationSeconds())
                                    .RoleArn(profile.GetRoleArn())
                                    .RoleSessionName(profile.GetRoleSessionName())
                                    .OIDCProviderArn(profile.GetOidcProviderArn())
                                    .OIDCTokenFilePath(profile.GetOidcTokenFile())
                                    .StsRegionId(profile.GetStsRegionId())
                                    .Policy(profile.GetPolicy())
                                    .EnableVpc(profile.GetEnableVpc())
                                    .Build();
                            case "ChainableRamRoleArn":
                                if (profile.GetSourceProfile() == profile.GetName())
                                {
                                    throw new CredentialException("Source profile name can not be the same as profile name.");
                                }
                                IAlibabaCloudCredentialsProvider previousProvider = ReloadCredentialsProvider(config, profile.GetSourceProfile());
                                return new RamRoleArnCredentialProvider.Builder()
                                    .CredentialsProvider(previousProvider)
                                    .RoleArn(profile.GetRoleArn())
                                    .DurationSeconds(profile.GetDurationSeconds())
                                    .RoleSessionName(profile.GetRoleSessionName())
                                    .StsRegionId(profile.GetStsRegionId())
                                    .EnableVpc(profile.GetEnableVpc())
                                    .Policy(profile.GetPolicy())
                                    .ExternalId(profile.GetExternalId())
                                    .Build();
                            default:
                                throw new CredentialException(string.Format("Unsupported profile mode '{0}' form CLI credentials file.", profile.GetMode()));
                        }
                    }
                }
            }
            throw new CredentialException(string.Format("Unable to get profile with '{0}' form CLI credentials file.", currentProfileName));
        }

        internal Config ParseProfile(string configFilePath)
        {
            FileInfo configFile = new FileInfo(configFilePath);
            if (!configFile.Exists)
            {
                throw new CredentialException(string.Format("Unable to open credentials file: {0}.", configFile.FullName));
            }

            try
            {
                using (StreamReader sr = new StreamReader(configFile.FullName))
                {
                    StringBuilder sb = new StringBuilder();
                    string line;

                    while ((line = sr.ReadLine()) != null)
                    {
                        sb.Append(line);
                    }

                    string jsonContent = sb.ToString();
                    return JsonConvert.DeserializeObject<Config>(jsonContent);
                }
            }
            catch (Exception)
            {
                throw new CredentialException(string.Format("Failed to parse credential from CLI credentials file: {0}.", configFile.FullName));
            }
        }

        internal bool ShouldReloadCredentialsProvider(string profileName)
        {
            return credentialsProvider == null || (!string.IsNullOrEmpty(currentProfileName) && !string.IsNullOrEmpty(profileName) && !currentProfileName.Equals(profileName));
        }

        public string GetProviderName()
        {
            return "cli_profile";
        }

        internal class Config
        {
            [JsonProperty("current")]
            private readonly string current;
            [JsonProperty("profiles")]
            private readonly List<Profile> profiles;

            public string GetCurrent()
            {
                return current;
            }

            public List<Profile> GetProfiles()
            {
                return profiles;
            }
        }

        internal class Profile
        {
            [JsonProperty("name")]
            private readonly string name;
            [JsonProperty("mode")]
            private readonly string mode;
            [JsonProperty("access_key_id")]
            private readonly string accessKeyId;
            [JsonProperty("access_key_secret")]
            private readonly string accessKeySecret;
            [JsonProperty("ram_role_arn")]
            private readonly string roleArn;
            [JsonProperty("ram_session_name")]
            private readonly string roleSessionName;
            [JsonProperty("expired_seconds")]
            private readonly int? durationSeconds;
            [JsonProperty("sts_region")]
            private readonly string stsRegionId;
            [JsonProperty("ram_role_name")]
            private readonly string ramRoleName;
            [JsonProperty("oidc_token_file")]
            private readonly string oidcTokenFile;
            [JsonProperty("oidc_provider_arn")]
            private readonly string oidcProviderArn;
            [JsonProperty("source_profile")]
            private readonly string sourceProfile;
            [JsonProperty("policy")]
            private readonly string policy;
            [JsonProperty("region_id")]
            private readonly string regionId;
            [JsonProperty("enable_vpc")]
            private readonly bool? enableVpc;
            [JsonProperty("external_id")]
            private readonly string externalId;

            public string GetName()
            {
                return name;
            }
            public string GetMode()
            {
                return mode;
            }

            public string GetAccessKeyId()
            {
                return accessKeyId;
            }

            public string GetAccessKeySecret()
            {
                return accessKeySecret;
            }

            public string GetRoleArn()
            {
                return roleArn;
            }

            public string GetRoleSessionName()
            {
                return roleSessionName;
            }

            public int? GetDurationSeconds()
            {
                return durationSeconds;
            }

            public string GetStsRegionId()
            {
                return stsRegionId;
            }

            public string GetRamRoleName()
            {
                return ramRoleName;
            }

            public string GetOidcTokenFile()
            {
                return oidcTokenFile;
            }

            public string GetOidcProviderArn()
            {
                return oidcProviderArn;
            }

            public string GetSourceProfile()
            {
                return sourceProfile;
            }

            public string GetPolicy()
            {
                return policy;
            }

            public string GetRegionId()
            {
                return regionId;
            }

            public bool? GetEnableVpc()
            {
                return enableVpc;
            }

            public string GetExternalId()
            {
                return externalId;
            }
        }
    }
}