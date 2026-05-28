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
        private static readonly Dictionary<string, string> OAuthBaseUrlMap = new Dictionary<string, string>
        {
            { "CN", "https://oauth.aliyun.com" },
            { "INTL", "https://oauth.alibabacloud.com" }
        };

        private static readonly Dictionary<string, string> OAuthClientMap = new Dictionary<string, string>
        {
            { "CN", "4038181954557748008" },
            { "INTL", "4103531455503354461" }
        };

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
                                return new StaticAKCredentialsProvider.Builder()
                                    .AccessKeyId(profile.GetAccessKeyId())
                                    .AccessKeySecret(profile.GetAccessKeySecret())
                                    .Build();
                            case "StsToken":
                                return new StaticSTSCredentialsProvider.Builder()
                                    .AccessKeyId(profile.GetAccessKeyId())
                                    .AccessKeySecret(profile.GetAccessKeySecret())
                                    .SecurityToken(profile.GetSecurityToken())
                                    .Build();
                            case "RamRoleArn":
                                CredentialModel credentialModel = new CredentialModel
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
                            case "CloudSSO":
                                return new CloudSSOCredentialProvider.Builder()
                                    .SignInUrl(profile.GetSignInUrl())
                                    .AccountId(profile.GetAccountId())
                                    .AccessConfig(profile.GetAccessConfig())
                                    .AccessToken(profile.GetAccessToken())
                                    .AccessTokenExpire(profile.GetAccessTokenExpire())
                                    .Build();
                            case "OAuth":
                                string siteType = (profile.GetOauthSiteType() ?? "").ToUpper();
                                if (!OAuthBaseUrlMap.ContainsKey(siteType))
                                {
                                    throw new CredentialException("Invalid OAuth site type, support CN or INTL.");
                                }
                                string oauthSignInUrl = OAuthBaseUrlMap[siteType];
                                string oauthClientId = OAuthClientMap[siteType];
                                return new OAuthCredentialProvider.Builder()
                                    .SignInUrl(oauthSignInUrl)
                                    .ClientId(oauthClientId)
                                    .RefreshToken(profile.GetOauthRefreshToken())
                                    .AccessToken(profile.GetOauthAccessToken())
                                    .AccessTokenExpire(profile.GetOauthAccessTokenExpire())
                                    .TokenUpdateCallback(CreateOAuthTokenUpdateCallback(currentProfileName))
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

        private OAuthTokenUpdateCallback CreateOAuthTokenUpdateCallback(string profileName)
        {
            return (refreshToken, accessToken, accessKeyId, accessKeySecret, securityToken, accessTokenExpire, stsExpire) =>
            {
                try
                {
                    UpdateOAuthTokens(profileName, refreshToken, accessToken, accessKeyId, accessKeySecret, securityToken, accessTokenExpire, stsExpire);
                }
                catch (Exception)
                {
                    // Warning only
                }
            };
        }

        private void UpdateOAuthTokens(string profileName, string refreshToken, string accessToken,
            string accessKeyId, string accessKeySecret, string securityToken,
            long accessTokenExpire, long stsExpire)
        {
            string configPath = CLI_CREDENTIALS_CONFIG_PATH;
            if (!File.Exists(configPath)) return;

            string jsonContent = File.ReadAllText(configPath);
            Config config = JsonConvert.DeserializeObject<Config>(jsonContent);
            if (config == null || config.GetProfiles() == null) return;

            string name = !string.IsNullOrEmpty(profileName) ? profileName : config.GetCurrent();
            Profile oauthProfile = FindOAuthProfile(config, name);
            if (oauthProfile == null) return;

            oauthProfile.SetOauthRefreshToken(refreshToken);
            oauthProfile.SetOauthAccessToken(accessToken);
            oauthProfile.SetOauthAccessTokenExpire(accessTokenExpire);
            oauthProfile.SetAccessKeyId(accessKeyId);
            oauthProfile.SetAccessKeySecret(accessKeySecret);
            oauthProfile.SetSecurityToken(securityToken);
            oauthProfile.SetStsExpire(stsExpire);

            string updatedJson = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(configPath, updatedJson);
        }

        private Profile FindOAuthProfile(Config config, string profileName)
        {
            if (config.GetProfiles() == null) return null;
            foreach (var p in config.GetProfiles())
            {
                if (p.GetName() == profileName)
                {
                    if (p.GetMode() == "OAuth") return p;
                    if (!string.IsNullOrEmpty(p.GetSourceProfile()))
                        return FindOAuthProfile(config, p.GetSourceProfile());
                    return null;
                }
            }
            return null;
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
            private string accessKeyId;
            [JsonProperty("access_key_secret")]
            private string accessKeySecret;
            [JsonProperty("sts_token")]
            private string securityToken;
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
            [JsonProperty("cloud_sso_sign_in_url")]
            private readonly string signInUrl;
            [JsonProperty("cloud_sso_account_id")]
            private readonly string accountId;
            [JsonProperty("cloud_sso_access_config")]
            private readonly string accessConfig;
            [JsonProperty("access_token")]
            private readonly string accessToken;
            [JsonProperty("cloud_sso_access_token_expire")]
            private readonly long accessTokenExpire;
            [JsonProperty("oauth_site_type")]
            private readonly string oauthSiteType;
            [JsonProperty("oauth_refresh_token")]
            private string oauthRefreshToken;
            [JsonProperty("oauth_access_token")]
            private string oauthAccessToken;
            [JsonProperty("oauth_access_token_expire")]
            private long oauthAccessTokenExpire;
            [JsonProperty("sts_expiration")]
            private long stsExpire;

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

            public string GetSecurityToken()
            {
                return securityToken;
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

            public string GetSignInUrl()
            {
                return signInUrl;
            }

            public string GetAccountId()
            {
                return accountId;
            }

            public string GetAccessConfig()
            {
                return accessConfig;
            }

            public string GetAccessToken()
            {
                return accessToken;
            }

            public long GetAccessTokenExpire()
            {
                return accessTokenExpire;
            }

            public string GetOauthSiteType()
            {
                return oauthSiteType;
            }

            public string GetOauthRefreshToken()
            {
                return oauthRefreshToken;
            }

            public string GetOauthAccessToken()
            {
                return oauthAccessToken;
            }

            public long GetOauthAccessTokenExpire()
            {
                return oauthAccessTokenExpire;
            }

            public void SetOauthRefreshToken(string value) { oauthRefreshToken = value; }
            public void SetOauthAccessToken(string value) { oauthAccessToken = value; }
            public void SetOauthAccessTokenExpire(long value) { oauthAccessTokenExpire = value; }
            public void SetAccessKeyId(string value) { accessKeyId = value; }
            public void SetAccessKeySecret(string value) { accessKeySecret = value; }
            public void SetSecurityToken(string value) { securityToken = value; }
            public void SetStsExpire(long value) { stsExpire = value; }
        }
    }
}