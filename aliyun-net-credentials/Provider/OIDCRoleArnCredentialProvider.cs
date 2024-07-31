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
        private string roleSessionName = "defaultSessionName";
        private string regionId;
        private string policy;

        /// <summary>
        /// Unit of millisecond
        /// </summary>
        private int connectTimeout = 1000;
        private int readTimeout = 1000;

        /// <summary>
        /// Endpoint of RAM OpenAPI
        /// </summary>
        private string STSEndpoint = "sts.aliyuncs.com";
        public OIDCRoleArnCredentialProvider(Config config) : this(config.RoleArn, config.OIDCProviderArn, config.OIDCTokenFilePath)
        {
            roleSessionName = config.RoleSessionName;
            connectTimeout = config.ConnectTimeout;
            readTimeout = config.Timeout;
            policy = config.Policy;
            if (config.RoleSessionExpiration > 0)
            {
                durationSeconds = config.RoleSessionExpiration;
            }
        }

        public OIDCRoleArnCredentialProvider(string roleArn, string oidcProviderArn, string oidcTokenFilePath)
        {
            this.roleArn = ParameterHelper.ValidateNotNull(roleArn, "roleArn", "RoleArn must not be null.");
            this.oidcProviderArn = ParameterHelper.ValidateNotNull(oidcProviderArn, "oidcProviderArn", "OIDCProviderArn must not be null.");
            this.oidcTokenFilePath = ParameterHelper.ValidateNotNull(oidcTokenFilePath, "oidcTokenFilePath", "OIDCTokenFilePath must not be null.");
        }

        public OIDCRoleArnCredentialProvider(string roleArn, string oidcProviderArn, string oidcTokenFilePath, 
            string roleSessionName, string regionId, string policy) : this(roleArn, oidcProviderArn, oidcTokenFilePath)
        {
            this.roleSessionName = roleSessionName;
            this.regionId = regionId;
            this.policy = policy;
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
                    Type = AuthConstant.OIDCRoleArn
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
                    Type = AuthConstant.OIDCRoleArn
                };
                return new RefreshResult<CredentialModel>(credentialModel, GetStaleTime(expiration));
            }

            throw new CredentialException(JsonConvert.SerializeObject(map));
        }
    }
}