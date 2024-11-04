using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Http;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Utils;

using Newtonsoft.Json;

namespace Aliyun.Credentials.Provider
{
    /// <summary>
    /// <para>By specifying <see href="https://ram.console.aliyun.com/#/role/list">RAM Role</see>, the credential will be able to automatically request maintenance of STS Token.</para> 
    /// <para>If you want to limit the permissions of STS Token, you can assign value for Policy.</para>
    /// </summary>
    public class RamRoleArnCredentialProvider : SessionCredentialsProvider
    {
        /// <summary>
        /// Default duration for started sessions. Unit of Second
        /// </summary>
        public int durationSeconds = 3600;

        /// <summary>
        /// The arn of the role to be assumed.
        /// </summary>
        private string roleArn;

        /// <summary>
        /// An identifier for the assumed role session.
        /// </summary>
        private string roleSessionName = "credentials-csharp-" + (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

        private string regionId = "cn-hangzhou";
        private string policy;

        /// <summary>
        /// Unit of millisecond
        /// </summary>
        private int connectTimeout = 5000;

        private int readTimeout = 10000;

        /// <summary>
        /// Endpoint of RAM OpenAPI
        /// </summary>
        private string STSEndpoint = "sts.aliyuncs.com";

        private string externalId;

        public IAlibabaCloudCredentialsProvider CredentialsProvider { get; set; }

        [Obsolete("Use builder instead.")]
        public RamRoleArnCredentialProvider(Config config)
        {
            if (!string.IsNullOrEmpty(config.SecurityToken))
            {
                CredentialsProvider = new StaticSTSCredentialsProvider(config);
            }
            else
            {
                CredentialsProvider = new StaticAKCredentialsProvider(config);
            }
            roleArn = config.RoleArn ?? Environment.GetEnvironmentVariable("ALIBABA_CLOUD_ROLE_ARN");
            connectTimeout = config.ConnectTimeout > 0 ? config.ConnectTimeout : connectTimeout;
            readTimeout = config.Timeout > 0 ? config.Timeout : readTimeout;
            durationSeconds = config.RoleSessionExpiration > 0 ? config.RoleSessionExpiration : durationSeconds;
            policy = config.Policy;
            roleSessionName = config.RoleSessionName ?? Environment.GetEnvironmentVariable("ALIBABA_CLOUD_ROLE_SESSION_NAME") ?? roleSessionName;
            STSEndpoint = config.STSEndpoint ?? STSEndpoint;
            externalId = config.ExternalId;
        }

        [Obsolete("Use builder instead.")]
        public RamRoleArnCredentialProvider(string accessKeyId, string accessKeySecret, string roleArn)
        {
            CredentialsProvider = new StaticAKCredentialsProvider(accessKeyId, accessKeySecret);
            this.roleArn = roleArn ?? Environment.GetEnvironmentVariable("ALIBABA_CLOUD_ROLE_ARN");
        }

        [Obsolete("Use builder instead.")]
        public RamRoleArnCredentialProvider(IAlibabaCloudCredentialsProvider provider, string roleArn)
        {
            CredentialsProvider = ParameterHelper.ValidateNotNull(provider, "Provider", "Must specify a previous credentials provider to asssume role.");
            this.roleArn = roleArn ?? Environment.GetEnvironmentVariable("ALIBABA_CLOUD_ROLE_ARN");
        }

        [Obsolete("Use builder instead.")]
        public RamRoleArnCredentialProvider(IAlibabaCloudCredentialsProvider provider, string roleArn, int durationSeconds,
            string roleSessionName) : this(provider, roleArn)
        {
            this.roleSessionName = roleSessionName ?? Environment.GetEnvironmentVariable("ALIBABA_CLOUD_ROLE_SESSION_NAME") ?? this.roleSessionName;
            this.durationSeconds = durationSeconds;
        }

        [Obsolete("Use builder instead.")]
        public RamRoleArnCredentialProvider(string accessKeyId, string accessKeySecret, string roleSessionName,
            string roleArn, string regionId, string policy) : this(accessKeyId, accessKeySecret, roleArn)
        {
            this.roleSessionName = roleSessionName ?? Environment.GetEnvironmentVariable("ALIBABA_CLOUD_ROLE_SESSION_NAME") ?? this.roleSessionName;
            this.regionId = regionId;
            this.policy = policy;
        }

        [Obsolete("Use builder instead.")]
        public RamRoleArnCredentialProvider(IAlibabaCloudCredentialsProvider provider, string roleSessionName,
                    string roleArn, string regionId, string policy) : this(provider, roleArn)
        {
            this.roleSessionName = roleSessionName ?? Environment.GetEnvironmentVariable("ALIBABA_CLOUD_ROLE_SESSION_NAME") ?? this.roleSessionName;
            this.regionId = regionId;
            this.policy = policy;
        }

        private RamRoleArnCredentialProvider(Builder builder)
        {
            this.durationSeconds = builder.durationSeconds > 0 ? builder.durationSeconds : 3600;
            this.roleSessionName = string.IsNullOrEmpty(builder.roleSessionName)
                ? Environment.GetEnvironmentVariable("ALIBABA_CLOUD_ROLE_SESSION_NAME")
                    ?? "credentials-csharp-" + (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds
                : builder.roleSessionName;
            this.regionId = string.IsNullOrEmpty(builder.regionId) ? "cn-hangzhou" : builder.regionId;
            this.roleArn = string.IsNullOrEmpty(builder.roleArn) ? Environment.GetEnvironmentVariable("ALIBABA_CLOUD_ROLE_ARN") : builder.roleArn;
            this.policy = builder.policy;
            this.connectTimeout = builder.connectTimeout > 0 ? builder.connectTimeout : 5000;
            this.readTimeout = builder.readTimeout > 0 ? builder.readTimeout : 10000;
            this.externalId = builder.externalId;
            if (string.IsNullOrEmpty(builder.stsEndpoint))
            {
                this.STSEndpoint = string.Format("sts{0}.aliyuncs.com", AuthUtils.GetStsRegionWithVpc(builder.stsRegionId, builder.enableVpc));
            }
            else
            {
                this.STSEndpoint = string.IsNullOrEmpty(builder.stsEndpoint) ? "sts.aliyuncs.com" : builder.stsEndpoint;
            }
            if (builder.credentialsProvider != null)
            {
                this.CredentialsProvider = builder.credentialsProvider;
            }
            else if (builder.securityToken == null)
            {
                this.CredentialsProvider = new StaticAKCredentialsProvider.Builder()
                    .AccessKeyId(builder.accessKeyId)
                    .AccessKeySecret(builder.accessKeySecret)
                    .Build();
            }
            else
            {
                this.CredentialsProvider = new StaticSTSCredentialsProvider.Builder()
                    .AccessKeyId(builder.accessKeyId)
                    .AccessKeySecret(builder.accessKeySecret)
                    .SecurityToken(builder.securityToken)
                    .Build();
            }
        }

        public class Builder
        {
            internal int durationSeconds;
            internal string roleSessionName;
            internal string regionId;
            internal string roleArn;
            internal string policy;
            internal int connectTimeout;
            internal int readTimeout;
            internal string stsEndpoint;
            internal IAlibabaCloudCredentialsProvider credentialsProvider;
            internal string externalId;
            internal string stsRegionId;
            internal bool? enableVpc;
            internal string accessKeyId;
            internal string accessKeySecret;
            internal string securityToken;

            public Builder DurationSeconds(int durationSeconds)
            {
                this.durationSeconds = durationSeconds;
                return this;
            }

            public Builder RoleSessionName(string roleSessionName)
            {
                this.roleSessionName = roleSessionName;
                return this;
            }

            public Builder RegionId(string regionId)
            {
                this.regionId = regionId;
                return this;
            }

            public Builder RoleArn(string roleArn)
            {
                this.roleArn = roleArn;
                return this;
            }

            public Builder Policy(string policy)
            {
                this.policy = policy;
                return this;
            }

            public Builder ConnectTimeout(int connectTimeout)
            {
                this.connectTimeout = connectTimeout;
                return this;
            }

            public Builder ReadTimeout(int readTimeout)
            {
                this.readTimeout = readTimeout;
                return this;
            }

            public Builder STSEndpoint(string stsEndpoint)
            {
                this.stsEndpoint = stsEndpoint;
                return this;
            }

            public Builder CredentialsProvider(IAlibabaCloudCredentialsProvider credentialsProvider)
            {
                this.credentialsProvider = credentialsProvider;
                return this;
            }

            public Builder ExternalId(string externalId)
            {
                this.externalId = externalId;
                return this;
            }

            public Builder StsRegionId(string stsRegionId)
            {
                this.stsRegionId = stsRegionId;
                return this;
            }

            public Builder EnableVpc(bool enableVpc)
            {
                this.enableVpc = enableVpc;
                return this;
            }

            public Builder AccessKeyId(string accessKeyId)
            {
                this.accessKeyId = accessKeyId;
                return this;
            }

            public Builder AccessKeySecret(string accessKeySecret)
            {
                this.accessKeySecret = accessKeySecret;
                return this;
            }

            public Builder SecurityToken(string securityToken)
            {
                this.securityToken = securityToken;
                return this;
            }

            public RamRoleArnCredentialProvider Build()
            {
                return new RamRoleArnCredentialProvider(this);
            }
        }

        public override RefreshResult<CredentialModel> RefreshCredentials()
        {
            CompatibleUrlConnClient client = new CompatibleUrlConnClient();
            return CreateCredential(client);
        }

        public override async Task<RefreshResult<CredentialModel>> RefreshCredentialsAsync()
        {
            CompatibleUrlConnClient client = new CompatibleUrlConnClient();
            return await CreateCredentialAsync(client);
        }

        private RefreshResult<CredentialModel> CreateCredential(IConnClient client)
        {
            return GetNewSessionCredentials(client);
        }

        private async Task<RefreshResult<CredentialModel>> CreateCredentialAsync(IConnClient client)
        {
            return await GetNewSessionCredentialsAsync(client);
        }

        private RefreshResult<CredentialModel> GetNewSessionCredentials(IConnClient client)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.SetCommonUrlParameters();
            httpRequest.AddUrlParameter("Action", "AssumeRole");
            httpRequest.AddUrlParameter("Format", "JSON");
            httpRequest.AddUrlParameter("Version", "2015-04-01");
            httpRequest.AddUrlParameter("DurationSeconds", durationSeconds.ToString());
            httpRequest.AddUrlParameter("RoleArn", this.roleArn);
            CredentialModel previousCredentials = CredentialsProvider.GetCredentials();
            ParameterHelper.ValidateNotNull(previousCredentials, "OriginalCredentials", "Unable to load original credentials from the providers in RAM role arn.");
            httpRequest.AddUrlParameter("AccessKeyId", previousCredentials.AccessKeyId);
            httpRequest.AddUrlParameter("SecurityToken", previousCredentials.SecurityToken);
            httpRequest.AddUrlParameter("RoleSessionName", this.roleSessionName);
            if (policy != null)
            {
                httpRequest.AddUrlParameter("Policy", this.policy);
            }

            if (externalId != null)
            {
                httpRequest.AddUrlParameter("ExternalId", this.externalId);
            }

            httpRequest.Method = MethodType.GET;
            httpRequest.ConnectTimeout = connectTimeout;
            httpRequest.ReadTimeout = readTimeout;
            string strToSign = ParameterHelper.ComposeStringToSign(MethodType.GET, httpRequest.UrlParameters);
            string signature = ParameterHelper.SignString(strToSign, previousCredentials.AccessKeySecret + "&");
            httpRequest.AddUrlParameter("Signature", signature);
            httpRequest.Url = ParameterHelper.ComposeUrl(STSEndpoint, httpRequest.UrlParameters,
                "https");
            HttpResponse httpResponse = client.DoAction(httpRequest);
            Dictionary<string, object> map =
                JsonConvert.DeserializeObject<Dictionary<string, object>>(httpResponse.GetHttpContentString());
            if (map.ContainsKey("Credentials"))
            {
                string credentialsJson = JsonConvert.SerializeObject(DictionaryUtil.Get(map, "Credentials"));
                Dictionary<string, string> credentials =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(credentialsJson);
                string expirationStr =
                    DictionaryUtil.Get(credentials, "Expiration").Replace('T', ' ').Replace('Z', ' ');
                var dt = Convert.ToDateTime(expirationStr);
                long expiration = dt.GetTimeMillis();
                CredentialModel credentialModel = new CredentialModel
                {
                    AccessKeyId = DictionaryUtil.Get(credentials, "AccessKeyId"),
                    AccessKeySecret = DictionaryUtil.Get(credentials, "AccessKeySecret"),
                    SecurityToken = DictionaryUtil.Get(credentials, "SecurityToken"),
                    Expiration = expiration,
                    Type = AuthConstant.RamRoleArn,
                    ProviderName = string.Format("{0}/{1}", this.GetProviderName(), CredentialsProvider.GetProviderName())
                };
                return new RefreshResult<CredentialModel>(credentialModel, GetStaleTime(expiration));
            }

            throw new CredentialException(JsonConvert.SerializeObject(map));
        }

        private async Task<RefreshResult<CredentialModel>> GetNewSessionCredentialsAsync(IConnClient client)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.SetCommonUrlParameters();
            httpRequest.AddUrlParameter("Action", "AssumeRole");
            httpRequest.AddUrlParameter("Format", "JSON");
            httpRequest.AddUrlParameter("Version", "2015-04-01");
            httpRequest.AddUrlParameter("DurationSeconds", durationSeconds.ToString());
            httpRequest.AddUrlParameter("RoleArn", this.roleArn);
            CredentialModel previousCredentials = await CredentialsProvider.GetCredentialsAsync();
            ParameterHelper.ValidateNotNull(previousCredentials, "OriginalCredentials", "Unable to load original credentials from the providers in RAM role arn.");
            httpRequest.AddUrlParameter("AccessKeyId", previousCredentials.AccessKeyId);
            httpRequest.AddUrlParameter("SecurityToken", previousCredentials.SecurityToken);
            httpRequest.AddUrlParameter("RoleSessionName", this.roleSessionName);
            if (policy != null)
            {
                httpRequest.AddUrlParameter("Policy", this.policy);
            }

            if (externalId != null)
            {
                httpRequest.AddUrlParameter("ExternalId", this.externalId);
            }

            httpRequest.Method = MethodType.GET;
            httpRequest.ConnectTimeout = connectTimeout;
            httpRequest.ReadTimeout = readTimeout;
            string strToSign = ParameterHelper.ComposeStringToSign(MethodType.GET, httpRequest.UrlParameters);
            string signature = ParameterHelper.SignString(strToSign, previousCredentials.AccessKeySecret + "&");
            httpRequest.AddUrlParameter("Signature", signature);
            httpRequest.Url = ParameterHelper.ComposeUrl(STSEndpoint, httpRequest.UrlParameters,
                "https");
            HttpResponse httpResponse = await client.DoActionAsync(httpRequest);
            Dictionary<string, object> map =
                JsonConvert.DeserializeObject<Dictionary<string, object>>(httpResponse.GetHttpContentString());
            if (map.ContainsKey("Credentials"))
            {
                string credentialsJson = JsonConvert.SerializeObject(DictionaryUtil.Get(map, "Credentials"));
                Dictionary<string, string> credentials =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(credentialsJson);
                string expirationStr =
                    DictionaryUtil.Get(credentials, "Expiration").Replace('T', ' ').Replace('Z', ' ');
                var dt = Convert.ToDateTime(expirationStr);
                long expiration = dt.GetTimeMillis();
                CredentialModel credentialModel = new CredentialModel
                {
                    AccessKeyId = DictionaryUtil.Get(credentials, "AccessKeyId"),
                    AccessKeySecret = DictionaryUtil.Get(credentials, "AccessKeySecret"),
                    SecurityToken = DictionaryUtil.Get(credentials, "SecurityToken"),
                    Expiration = expiration,
                    Type = AuthConstant.RamRoleArn,
                    ProviderName = string.Format("{0}/{1}", this.GetProviderName(), CredentialsProvider.GetProviderName())
                };
                return new RefreshResult<CredentialModel>(credentialModel, GetStaleTime(expiration));
            }

            throw new CredentialException(JsonConvert.SerializeObject(map));
        }

        public override string GetProviderName()
        {
            return "ram_role_arn";
        }

        public string GetSTSEndpoint()
        {
            return this.STSEndpoint;
        }

        public string GetExternalId()
        {
            return this.externalId;
        }
    }
}
