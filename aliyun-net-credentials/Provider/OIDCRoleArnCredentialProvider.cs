using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Http;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Utils;

using Newtonsoft.Json;

namespace Aliyun.Credentials.Provider
{
    public class OIDCRoleArnCredentialProvider : SessionCredentialsProvider
    {
        /// <summary>
        /// Default duration for started sessions. Unit of Second
        /// </summary>
        public int durationSeconds = 3600;

        /// <summary>
        /// The arn of the role to be assumed.
        /// </summary>
        private string roleArn;
        private string oidcProviderArn;
        private string oidcToken;
        private string oidcTokenFilePath;

        /// <summary>
        /// An identifier for the assumed role session.
        /// </summary>
        private string roleSessionName = "credentials-csharp-" + (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        private string regionId;
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

        [Obsolete("Use builder instead.")]
        public OIDCRoleArnCredentialProvider(Config config) : this(config.RoleArn, config.OIDCProviderArn, config.OIDCTokenFilePath)
        {
            roleSessionName = config.RoleSessionName ?? Environment.GetEnvironmentVariable("ALIBABA_CLOUD_ROLE_SESSION_NAME") ?? roleSessionName;
            connectTimeout = config.ConnectTimeout;
            readTimeout = config.Timeout;
            policy = config.Policy;
            if (config.RoleSessionExpiration > 0)
            {
                durationSeconds = config.RoleSessionExpiration;
            }
            STSEndpoint = config.STSEndpoint ?? STSEndpoint;
        }

        [Obsolete("Use builder instead.")]
        public OIDCRoleArnCredentialProvider(string roleArn, string oidcProviderArn, string oidcTokenFilePath)
        {
            this.roleArn = ParameterHelper.ValidateEnvNotEmpty(roleArn, "ALIBABA_CLOUD_ROLE_ARN", "roleArn", "RoleArn must not be null or empty.");
            this.oidcProviderArn = ParameterHelper.ValidateEnvNotEmpty(oidcProviderArn, "ALIBABA_CLOUD_OIDC_PROVIDER_ARN", "oidcProviderArn", "OIDCProviderArn must not be null or empty.");
            this.oidcTokenFilePath = ParameterHelper.ValidateEnvNotEmpty(oidcTokenFilePath, "ALIBABA_CLOUD_OIDC_TOKEN_FILE", "oidcTokenFilePath", "OIDCTokenFilePath must not be null or empty.");
        }

        [Obsolete("Use builder instead.")]
        public OIDCRoleArnCredentialProvider(string roleArn, string oidcProviderArn, string oidcTokenFilePath,
            string roleSessionName, string regionId, string policy) : this(roleArn, oidcProviderArn, oidcTokenFilePath)
        {
            this.roleSessionName = roleSessionName ?? Environment.GetEnvironmentVariable("ALIBABA_CLOUD_ROLE_SESSION_NAME") ?? this.roleSessionName;
            this.regionId = regionId ?? this.regionId;
            this.policy = policy;
        }

        private OIDCRoleArnCredentialProvider(Builder builder)
        {
            this.durationSeconds = builder.durationSeconds > 0 ? builder.durationSeconds : 3600;
            this.roleSessionName = string.IsNullOrEmpty(builder.roleSessionName)
                ? Environment.GetEnvironmentVariable("ALIBABA_CLOUD_ROLE_SESSION_NAME")
                    ?? "credentials-csharp-" + (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds
                : builder.roleSessionName;
            this.regionId = string.IsNullOrEmpty(builder.regionId) ? "cn-hangzhou" : builder.regionId;
            this.roleArn = string.IsNullOrEmpty(builder.roleArn) ? Environment.GetEnvironmentVariable("ALIBABA_CLOUD_ROLE_ARN") : builder.roleArn;
            this.connectTimeout = builder.connectTimeout > 0 ? builder.connectTimeout : 5000;
            this.readTimeout = builder.readTimeout > 0 ? builder.readTimeout : 10000;
            this.oidcProviderArn = string.IsNullOrEmpty(builder.oidcProviderArn)
                ? Environment.GetEnvironmentVariable("ALIBABA_CLOUD_OIDC_PROVIDER_ARN")
                : builder.oidcProviderArn;
            this.oidcTokenFilePath = string.IsNullOrEmpty(builder.oidcTokenFilePath)
                ? Environment.GetEnvironmentVariable("ALIBABA_CLOUD_OIDC_TOKEN_FILE")
                : builder.oidcTokenFilePath;
            this.policy = builder.policy;
            if (string.IsNullOrEmpty(builder.stsEndpoint))
            {
                this.STSEndpoint = string.Format("sts{0}.aliyuncs.com", AuthUtils.GetStsRegionWithVpc(builder.stsRegionId, builder.enableVpc));
            }
            else
            {
                this.STSEndpoint = string.IsNullOrEmpty(builder.stsEndpoint) ? "sts.aliyuncs.com" : builder.stsEndpoint;
            }
        }

        public class Builder
        {
            internal int durationSeconds;
            internal string roleSessionName;
            internal string regionId;
            internal string roleArn;
            internal string oidcProviderArn;
            internal string oidcTokenFilePath;
            internal string policy;
            internal int connectTimeout;
            internal int readTimeout;
            internal string stsEndpoint;
            internal string stsRegionId;
            internal bool? enableVpc;
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

            public Builder OIDCProviderArn(string oidcProviderArn)
            {
                this.oidcProviderArn = oidcProviderArn;
                return this;
            }

            public Builder OIDCTokenFilePath(string oidcTokenFilePath)
            {
                this.oidcTokenFilePath = oidcTokenFilePath;
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

            public OIDCRoleArnCredentialProvider Build()
            {
                return new OIDCRoleArnCredentialProvider(this);
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
            oidcToken = AuthUtils.GetOIDCToken(oidcTokenFilePath);
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.SetCommonUrlParameters();
            httpRequest.AddUrlParameter("Action", "AssumeRoleWithOIDC");
            httpRequest.AddUrlParameter("Format", "JSON");
            httpRequest.AddUrlParameter("Version", "2015-04-01");
            var body = new Dictionary<string, string>
            {
                { "DurationSeconds", durationSeconds.ToString() },
                { "RoleArn", roleArn },
                { "OIDCProviderArn", oidcProviderArn },
                { "OIDCToken", oidcToken },
                { "RoleSessionName", roleSessionName },
                { "Policy", policy },
            };
            bool first = true;
            var content = new StringBuilder();
            foreach (var entry in body)
            {
                if (string.IsNullOrEmpty(entry.Value))
                {
                    continue;
                }
                if (first)
                {
                    first = false;
                }
                else
                {
                    content.Append("&");
                }
                content.Append(System.Web.HttpUtility.UrlEncode(entry.Key));
                content.Append("=");
                content.Append(System.Web.HttpUtility.UrlEncode(entry.Value));
            }
            httpRequest.SetHttpContent(Encoding.UTF8.GetBytes(content.ToString()), "UTF-8", FormatType.Form);
            httpRequest.Method = MethodType.POST;
            httpRequest.ConnectTimeout = connectTimeout;
            httpRequest.ReadTimeout = readTimeout;
            httpRequest.Url = ParameterHelper.ComposeUrl(STSEndpoint, httpRequest.UrlParameters, "https");
            HttpResponse httpResponse = client.DoAction(httpRequest);
            Dictionary<string, object> map =
                JsonConvert.DeserializeObject<Dictionary<string, object>>(httpResponse.GetHttpContentString());
            if (map.ContainsKey("Credentials"))
            {
                string credentialsJson = JsonConvert.SerializeObject(DictionaryUtil.Get(map, "Credentials"));
                Dictionary<string, string> credentials =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(credentialsJson);
                string expirationStr = DictionaryUtil.Get(credentials, "Expiration").Replace('T', ' ').Replace('Z', ' ');
                var dt = Convert.ToDateTime(expirationStr);
                long expiration = dt.GetTimeMillis();
                CredentialModel credentialModel = new CredentialModel
                {
                    AccessKeyId = DictionaryUtil.Get(credentials, "AccessKeyId"),
                    AccessKeySecret = DictionaryUtil.Get(credentials, "AccessKeySecret"),
                    SecurityToken = DictionaryUtil.Get(credentials, "SecurityToken"),
                    Expiration = expiration,
                    Type = AuthConstant.OIDCRoleArn,
                    ProviderName = GetProviderName()
                };
                return new RefreshResult<CredentialModel>(credentialModel, GetStaleTime(expiration));
            }

            throw new CredentialException(JsonConvert.SerializeObject(map));
        }

        private async Task<RefreshResult<CredentialModel>> GetNewSessionCredentialsAsync(IConnClient client)
        {
            oidcToken = await AuthUtils.GetOIDCTokenAsync(oidcTokenFilePath);
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.SetCommonUrlParameters();
            httpRequest.AddUrlParameter("Action", "AssumeRoleWithOIDC");
            httpRequest.AddUrlParameter("Format", "JSON");
            httpRequest.AddUrlParameter("Version", "2015-04-01");
            var body = new Dictionary<string, string>
            {
                { "DurationSeconds", durationSeconds.ToString() },
                { "RoleArn", roleArn },
                { "OIDCProviderArn", oidcProviderArn },
                { "OIDCToken", oidcToken },
                { "RoleSessionName", roleSessionName },
                { "Policy", policy },
            };
            bool first = true;
            var content = new StringBuilder();
            foreach (var entry in body)
            {
                if (string.IsNullOrEmpty(entry.Value))
                {
                    continue;
                }
                if (first)
                {
                    first = false;
                }
                else
                {
                    content.Append("&");
                }
                content.Append(System.Web.HttpUtility.UrlEncode(entry.Key));
                content.Append("=");
                content.Append(System.Web.HttpUtility.UrlEncode(entry.Value));
            }
            httpRequest.SetHttpContent(Encoding.UTF8.GetBytes(content.ToString()), "UTF-8", FormatType.Form);
            httpRequest.Method = MethodType.POST;
            httpRequest.ConnectTimeout = connectTimeout;
            httpRequest.ReadTimeout = readTimeout;
            httpRequest.Url = ParameterHelper.ComposeUrl(STSEndpoint, httpRequest.UrlParameters, "https");
            HttpResponse httpResponse = await client.DoActionAsync(httpRequest);
            Dictionary<string, object> map =
                JsonConvert.DeserializeObject<Dictionary<string, object>>(httpResponse.GetHttpContentString());
            if (map.ContainsKey("Credentials"))
            {
                string credentialsJson = JsonConvert.SerializeObject(DictionaryUtil.Get(map, "Credentials"));
                Dictionary<string, string> credentials =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(credentialsJson);
                string expirationStr = DictionaryUtil.Get(credentials, "Expiration").Replace('T', ' ').Replace('Z', ' ');
                var dt = Convert.ToDateTime(expirationStr);
                long expiration = dt.GetTimeMillis();
                CredentialModel credentialModel = new CredentialModel
                {
                    AccessKeyId = DictionaryUtil.Get(credentials, "AccessKeyId"),
                    AccessKeySecret = DictionaryUtil.Get(credentials, "AccessKeySecret"),
                    SecurityToken = DictionaryUtil.Get(credentials, "SecurityToken"),
                    Expiration = expiration,
                    Type = AuthConstant.OIDCRoleArn,
                    ProviderName = GetProviderName()
                };
                return new RefreshResult<CredentialModel>(credentialModel, GetStaleTime(expiration));
            }

            throw new CredentialException(JsonConvert.SerializeObject(map));
        }

        public override string GetProviderName()
        {
            return "oidc_role_arn";
        }
    }
}