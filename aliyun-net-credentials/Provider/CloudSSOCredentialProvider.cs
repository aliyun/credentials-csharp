using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Http;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Utils;

using Newtonsoft.Json;

namespace Aliyun.Credentials.Provider
{
    public class CloudSSOCredentialProvider : SessionCredentialsProvider
    {
        private readonly string signInUrl;
        private readonly string accountId;
        private readonly string accessConfig;
        private readonly string accessToken;
        private readonly long accessTokenExpire;
        private readonly int connectTimeout;
        private readonly int readTimeout;

        private CloudSSOCredentialProvider(Builder builder) : base(builder)
        {
            long now = DateTime.UtcNow.GetTimeMillis() / 1000;
            if (string.IsNullOrEmpty(builder.accessToken) || builder.accessTokenExpire == 0
                || builder.accessTokenExpire - now <= 0)
            {
                throw new CredentialException("CloudSSO access token is empty or expired, please re-login with cli.");
            }
            if (string.IsNullOrEmpty(builder.signInUrl) || string.IsNullOrEmpty(builder.accountId)
                || string.IsNullOrEmpty(builder.accessConfig))
            {
                throw new CredentialException("CloudSSO sign in url, account id, and access config cannot be empty.");
            }

            this.signInUrl = builder.signInUrl;
            this.accountId = builder.accountId;
            this.accessConfig = builder.accessConfig;
            this.accessToken = builder.accessToken;
            this.accessTokenExpire = builder.accessTokenExpire;
            this.connectTimeout = builder.connectTimeout ?? 5000;
            this.readTimeout = builder.readTimeout ?? 10000;
        }

        public class Builder : SessionCredentialsProvider.Builder
        {
            internal string signInUrl;
            internal string accountId;
            internal string accessConfig;
            internal string accessToken;
            internal long accessTokenExpire;
            internal int? connectTimeout;
            internal int? readTimeout;

            public Builder SignInUrl(string signInUrl)
            {
                this.signInUrl = signInUrl;
                return this;
            }

            public Builder AccountId(string accountId)
            {
                this.accountId = accountId;
                return this;
            }

            public Builder AccessConfig(string accessConfig)
            {
                this.accessConfig = accessConfig;
                return this;
            }

            public Builder AccessToken(string accessToken)
            {
                this.accessToken = accessToken;
                return this;
            }

            public Builder AccessTokenExpire(long accessTokenExpire)
            {
                this.accessTokenExpire = accessTokenExpire;
                return this;
            }

            public Builder ConnectTimeout(int? connectTimeout)
            {
                this.connectTimeout = connectTimeout;
                return this;
            }

            public Builder ReadTimeout(int? readTimeout)
            {
                this.readTimeout = readTimeout;
                return this;
            }

            public CloudSSOCredentialProvider Build()
            {
                return new CloudSSOCredentialProvider(this);
            }
        }

        public override RefreshResult<CredentialModel> RefreshCredentials()
        {
            CompatibleUrlConnClient client = new CompatibleUrlConnClient();
            return GetNewSessionCredentials(client);
        }

        public override async Task<RefreshResult<CredentialModel>> RefreshCredentialsAsync()
        {
            CompatibleUrlConnClient client = new CompatibleUrlConnClient();
            return await GetNewSessionCredentialsAsync(client);
        }

        private RefreshResult<CredentialModel> GetNewSessionCredentials(IConnClient client)
        {
            Uri parsedUrl = new Uri(this.signInUrl);
            string requestUrl = parsedUrl.Scheme + "://" + parsedUrl.Host + "/cloud-credentials";

            string body = string.Format("{{\"AccountId\":\"{0}\",\"AccessConfigurationId\":\"{1}\"}}",
                this.accountId, this.accessConfig);

            HttpRequest httpRequest = new HttpRequest(requestUrl);
            httpRequest.Method = MethodType.POST;
            httpRequest.ConnectTimeout = this.connectTimeout;
            httpRequest.ReadTimeout = this.readTimeout;
            httpRequest.SetHttpContent(Encoding.UTF8.GetBytes(body), "UTF-8", FormatType.Json);
            httpRequest.Headers["Accept"] = "application/json";
            httpRequest.Headers["Content-Type"] = "application/json";
            httpRequest.Headers["Authorization"] = "Bearer " + this.accessToken;

            HttpResponse httpResponse = client.DoAction(httpRequest);

            if (httpResponse.Status != 200)
            {
                throw new CredentialException(string.Format(
                    "Get session token from CloudSSO failed, HttpCode: {0}, result: {1}.",
                    httpResponse.Status, httpResponse.GetHttpContentString()));
            }

            Dictionary<string, object> map =
                JsonConvert.DeserializeObject<Dictionary<string, object>>(httpResponse.GetHttpContentString());
            if (map == null || !map.ContainsKey("CloudCredential"))
            {
                throw new CredentialException(string.Format(
                    "Get session token from CloudSSO failed, result: {0}.", httpResponse.GetHttpContentString()));
            }

            string credentialsJson = JsonConvert.SerializeObject(DictionaryUtil.Get(map, "CloudCredential"));
            Dictionary<string, string> credentials =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(credentialsJson);

            if (credentials == null || !credentials.ContainsKey("AccessKeyId")
                || !credentials.ContainsKey("AccessKeySecret") || !credentials.ContainsKey("SecurityToken"))
            {
                throw new CredentialException(string.Format(
                    "Get session token from CloudSSO failed, fail to get credentials: {0}.",
                    httpResponse.GetHttpContentString()));
            }

            string expirationStr = DictionaryUtil.Get(credentials, "Expiration").Replace('T', ' ').Replace('Z', ' ');
            var dt = Convert.ToDateTime(expirationStr);
            long expiration = dt.GetTimeMillis();

            CredentialModel credentialModel = new CredentialModel
            {
                AccessKeyId = DictionaryUtil.Get(credentials, "AccessKeyId"),
                AccessKeySecret = DictionaryUtil.Get(credentials, "AccessKeySecret"),
                SecurityToken = DictionaryUtil.Get(credentials, "SecurityToken"),
                Expiration = expiration,
                Type = AuthConstant.Sts,
                ProviderName = GetProviderName()
            };
            return new RefreshResult<CredentialModel>(credentialModel, GetStaleTime(expiration));
        }

        private async Task<RefreshResult<CredentialModel>> GetNewSessionCredentialsAsync(IConnClient client)
        {
            Uri parsedUrl = new Uri(this.signInUrl);
            string requestUrl = parsedUrl.Scheme + "://" + parsedUrl.Host + "/cloud-credentials";

            string body = string.Format("{{\"AccountId\":\"{0}\",\"AccessConfigurationId\":\"{1}\"}}",
                this.accountId, this.accessConfig);

            HttpRequest httpRequest = new HttpRequest(requestUrl);
            httpRequest.Method = MethodType.POST;
            httpRequest.ConnectTimeout = this.connectTimeout;
            httpRequest.ReadTimeout = this.readTimeout;
            httpRequest.SetHttpContent(Encoding.UTF8.GetBytes(body), "UTF-8", FormatType.Json);
            httpRequest.Headers["Accept"] = "application/json";
            httpRequest.Headers["Content-Type"] = "application/json";
            httpRequest.Headers["Authorization"] = "Bearer " + this.accessToken;

            HttpResponse httpResponse = await client.DoActionAsync(httpRequest);

            if (httpResponse.Status != 200)
            {
                throw new CredentialException(string.Format(
                    "Get session token from CloudSSO failed, HttpCode: {0}, result: {1}.",
                    httpResponse.Status, httpResponse.GetHttpContentString()));
            }

            Dictionary<string, object> map =
                JsonConvert.DeserializeObject<Dictionary<string, object>>(httpResponse.GetHttpContentString());
            if (map == null || !map.ContainsKey("CloudCredential"))
            {
                throw new CredentialException(string.Format(
                    "Get session token from CloudSSO failed, result: {0}.", httpResponse.GetHttpContentString()));
            }

            string credentialsJson = JsonConvert.SerializeObject(DictionaryUtil.Get(map, "CloudCredential"));
            Dictionary<string, string> credentials =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(credentialsJson);

            if (credentials == null || !credentials.ContainsKey("AccessKeyId")
                || !credentials.ContainsKey("AccessKeySecret") || !credentials.ContainsKey("SecurityToken"))
            {
                throw new CredentialException(string.Format(
                    "Get session token from CloudSSO failed, fail to get credentials: {0}.",
                    httpResponse.GetHttpContentString()));
            }

            string expirationStr = DictionaryUtil.Get(credentials, "Expiration").Replace('T', ' ').Replace('Z', ' ');
            var dt = Convert.ToDateTime(expirationStr);
            long expiration = dt.GetTimeMillis();

            CredentialModel credentialModel = new CredentialModel
            {
                AccessKeyId = DictionaryUtil.Get(credentials, "AccessKeyId"),
                AccessKeySecret = DictionaryUtil.Get(credentials, "AccessKeySecret"),
                SecurityToken = DictionaryUtil.Get(credentials, "SecurityToken"),
                Expiration = expiration,
                Type = AuthConstant.Sts,
                ProviderName = GetProviderName()
            };
            return new RefreshResult<CredentialModel>(credentialModel, GetStaleTime(expiration));
        }

        public override string GetProviderName()
        {
            return "cloud_sso";
        }
    }
}
