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
    public delegate void OAuthTokenUpdateCallback(string refreshToken, string accessToken,
        string accessKeyId, string accessKeySecret, string securityToken,
        long accessTokenExpire, long stsExpire);

    public class OAuthCredentialProvider : SessionCredentialsProvider
    {
        private readonly string clientId;
        private readonly string signInUrl;
        private volatile string refreshToken;
        private volatile string accessToken;
        private long accessTokenExpire;
        private readonly int connectTimeout;
        private readonly int readTimeout;
        private readonly OAuthTokenUpdateCallback tokenUpdateCallback;

        private OAuthCredentialProvider(Builder builder) : base(builder)
        {
            if (string.IsNullOrEmpty(builder.clientId))
            {
                throw new CredentialException("The clientId is empty.");
            }
            if (string.IsNullOrEmpty(builder.signInUrl))
            {
                throw new CredentialException("The url for sign-in is empty.");
            }

            this.clientId = builder.clientId;
            this.signInUrl = builder.signInUrl;
            this.refreshToken = builder.refreshToken;
            this.accessToken = builder.accessToken;
            this.accessTokenExpire = builder.accessTokenExpire;
            this.connectTimeout = builder.connectTimeout ?? 5000;
            this.readTimeout = builder.readTimeout ?? 10000;
            this.tokenUpdateCallback = builder.tokenUpdateCallback;
        }

        public class Builder : SessionCredentialsProvider.Builder
        {
            internal string clientId;
            internal string signInUrl;
            internal string refreshToken;
            internal string accessToken;
            internal long accessTokenExpire;
            internal int? connectTimeout;
            internal int? readTimeout;
            internal OAuthTokenUpdateCallback tokenUpdateCallback;

            public Builder ClientId(string clientId)
            {
                this.clientId = clientId;
                return this;
            }

            public Builder SignInUrl(string signInUrl)
            {
                this.signInUrl = signInUrl;
                return this;
            }

            public Builder RefreshToken(string refreshToken)
            {
                this.refreshToken = refreshToken;
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

            public Builder TokenUpdateCallback(OAuthTokenUpdateCallback callback)
            {
                this.tokenUpdateCallback = callback;
                return this;
            }

            public OAuthCredentialProvider Build()
            {
                return new OAuthCredentialProvider(this);
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

        private void TryRefreshOAuthToken(IConnClient client)
        {
            Uri parsedUrl = new Uri(this.signInUrl);
            string requestUrl = parsedUrl.Scheme + "://" + parsedUrl.Host + "/v1/token";

            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
            string body = string.Format("grant_type=refresh_token&refresh_token={0}&client_id={1}&Timestamp={2}",
                Uri.EscapeDataString(this.refreshToken),
                Uri.EscapeDataString(this.clientId),
                Uri.EscapeDataString(timestamp));

            HttpRequest httpRequest = new HttpRequest(requestUrl);
            httpRequest.Method = MethodType.POST;
            httpRequest.ConnectTimeout = this.connectTimeout;
            httpRequest.ReadTimeout = this.readTimeout;
            httpRequest.SetHttpContent(Encoding.UTF8.GetBytes(body), "UTF-8", FormatType.Form);
            httpRequest.Headers["Content-Type"] = "application/x-www-form-urlencoded";

            HttpResponse httpResponse = client.DoAction(httpRequest);

            if (httpResponse.Status != 200)
            {
                throw new CredentialException(string.Format(
                    "Failed to refresh OAuth token, status code: {0}, result: {1}.",
                    httpResponse.Status, httpResponse.GetHttpContentString()));
            }

            Dictionary<string, object> tokenResp =
                JsonConvert.DeserializeObject<Dictionary<string, object>>(httpResponse.GetHttpContentString());
            if (tokenResp == null)
            {
                throw new CredentialException("Failed to refresh OAuth token: empty response.");
            }

            string newAccessToken = GetString(tokenResp, "access_token");
            string newRefreshToken = GetString(tokenResp, "refresh_token");
            long expiresIn = tokenResp.ContainsKey("expires_in") ? Convert.ToInt64(tokenResp["expires_in"]) : 3600;

            if (string.IsNullOrEmpty(newAccessToken) || string.IsNullOrEmpty(newRefreshToken))
            {
                throw new CredentialException(string.Format(
                    "Failed to refresh OAuth token: {0}.", httpResponse.GetHttpContentString()));
            }

            this.accessToken = newAccessToken;
            this.refreshToken = newRefreshToken;
            this.accessTokenExpire = GetUnixTimeSeconds() + expiresIn;
        }

        private async Task TryRefreshOAuthTokenAsync(IConnClient client)
        {
            Uri parsedUrl = new Uri(this.signInUrl);
            string requestUrl = parsedUrl.Scheme + "://" + parsedUrl.Host + "/v1/token";

            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
            string body = string.Format("grant_type=refresh_token&refresh_token={0}&client_id={1}&Timestamp={2}",
                Uri.EscapeDataString(this.refreshToken),
                Uri.EscapeDataString(this.clientId),
                Uri.EscapeDataString(timestamp));

            HttpRequest httpRequest = new HttpRequest(requestUrl);
            httpRequest.Method = MethodType.POST;
            httpRequest.ConnectTimeout = this.connectTimeout;
            httpRequest.ReadTimeout = this.readTimeout;
            httpRequest.SetHttpContent(Encoding.UTF8.GetBytes(body), "UTF-8", FormatType.Form);
            httpRequest.Headers["Content-Type"] = "application/x-www-form-urlencoded";

            HttpResponse httpResponse = await client.DoActionAsync(httpRequest);

            if (httpResponse.Status != 200)
            {
                throw new CredentialException(string.Format(
                    "Failed to refresh OAuth token, status code: {0}, result: {1}.",
                    httpResponse.Status, httpResponse.GetHttpContentString()));
            }

            Dictionary<string, object> tokenResp =
                JsonConvert.DeserializeObject<Dictionary<string, object>>(httpResponse.GetHttpContentString());
            if (tokenResp == null)
            {
                throw new CredentialException("Failed to refresh OAuth token: empty response.");
            }

            string newAccessToken = GetString(tokenResp, "access_token");
            string newRefreshToken = GetString(tokenResp, "refresh_token");
            long expiresIn = tokenResp.ContainsKey("expires_in") ? Convert.ToInt64(tokenResp["expires_in"]) : 3600;

            if (string.IsNullOrEmpty(newAccessToken) || string.IsNullOrEmpty(newRefreshToken))
            {
                throw new CredentialException(string.Format(
                    "Failed to refresh OAuth token: {0}.", httpResponse.GetHttpContentString()));
            }

            this.accessToken = newAccessToken;
            this.refreshToken = newRefreshToken;
            this.accessTokenExpire = GetUnixTimeSeconds() + expiresIn;
        }

        private RefreshResult<CredentialModel> GetNewSessionCredentials(IConnClient client)
        {
            long now = GetUnixTimeSeconds();
            if (!string.IsNullOrEmpty(this.refreshToken)
                && (string.IsNullOrEmpty(this.accessToken) || this.accessTokenExpire == 0
                    || this.accessTokenExpire - now <= 1200))
            {
                TryRefreshOAuthToken(client);
            }

            Uri parsedUrl = new Uri(this.signInUrl);
            string requestUrl = parsedUrl.Scheme + "://" + parsedUrl.Host + "/v1/exchange";

            HttpRequest httpRequest = new HttpRequest(requestUrl);
            httpRequest.Method = MethodType.POST;
            httpRequest.ConnectTimeout = this.connectTimeout;
            httpRequest.ReadTimeout = this.readTimeout;
            httpRequest.Headers["Content-Type"] = "application/json";
            httpRequest.Headers["Authorization"] = "Bearer " + this.accessToken;

            HttpResponse httpResponse = client.DoAction(httpRequest);

            if (httpResponse.Status != 200)
            {
                throw new CredentialException(string.Format(
                    "Get session token from OAuth failed, HttpCode: {0}, result: {1}.",
                    httpResponse.Status, httpResponse.GetHttpContentString()));
            }

            Dictionary<string, object> map =
                JsonConvert.DeserializeObject<Dictionary<string, object>>(httpResponse.GetHttpContentString());
            if (map == null)
            {
                throw new CredentialException(string.Format(
                    "Get session token from OAuth failed, result: {0}.", httpResponse.GetHttpContentString()));
            }

            string accessKeyId = GetString(map, "accessKeyId");
            string accessKeySecret = GetString(map, "accessKeySecret");
            string securityToken = GetString(map, "securityToken");
            string expirationStr = GetString(map, "expiration");

            if (string.IsNullOrEmpty(accessKeyId) || string.IsNullOrEmpty(accessKeySecret)
                || string.IsNullOrEmpty(securityToken))
            {
                throw new CredentialException(string.Format(
                    "Refresh session token from OAuth failed, fail to get credentials: {0}.",
                    httpResponse.GetHttpContentString()));
            }

            string cleanExpiration = expirationStr.Replace('T', ' ').Replace('Z', ' ');
            var dt = Convert.ToDateTime(cleanExpiration);
            long expiration = dt.GetTimeMillis();

            if (this.tokenUpdateCallback != null)
            {
                try
                {
                    this.tokenUpdateCallback(this.refreshToken, this.accessToken,
                        accessKeyId, accessKeySecret, securityToken,
                        this.accessTokenExpire, expiration / 1000);
                }
                catch (Exception)
                {
                    // Warning only
                }
            }

            CredentialModel credentialModel = new CredentialModel
            {
                AccessKeyId = accessKeyId,
                AccessKeySecret = accessKeySecret,
                SecurityToken = securityToken,
                Expiration = expiration,
                Type = AuthConstant.Sts,
                ProviderName = GetProviderName()
            };
            return new RefreshResult<CredentialModel>(credentialModel, GetStaleTime(expiration));
        }

        private async Task<RefreshResult<CredentialModel>> GetNewSessionCredentialsAsync(IConnClient client)
        {
            long now = GetUnixTimeSeconds();
            if (!string.IsNullOrEmpty(this.refreshToken)
                && (string.IsNullOrEmpty(this.accessToken) || this.accessTokenExpire == 0
                    || this.accessTokenExpire - now <= 1200))
            {
                await TryRefreshOAuthTokenAsync(client);
            }

            Uri parsedUrl = new Uri(this.signInUrl);
            string requestUrl = parsedUrl.Scheme + "://" + parsedUrl.Host + "/v1/exchange";

            HttpRequest httpRequest = new HttpRequest(requestUrl);
            httpRequest.Method = MethodType.POST;
            httpRequest.ConnectTimeout = this.connectTimeout;
            httpRequest.ReadTimeout = this.readTimeout;
            httpRequest.Headers["Content-Type"] = "application/json";
            httpRequest.Headers["Authorization"] = "Bearer " + this.accessToken;

            HttpResponse httpResponse = await client.DoActionAsync(httpRequest);

            if (httpResponse.Status != 200)
            {
                throw new CredentialException(string.Format(
                    "Get session token from OAuth failed, HttpCode: {0}, result: {1}.",
                    httpResponse.Status, httpResponse.GetHttpContentString()));
            }

            Dictionary<string, object> map =
                JsonConvert.DeserializeObject<Dictionary<string, object>>(httpResponse.GetHttpContentString());
            if (map == null)
            {
                throw new CredentialException(string.Format(
                    "Get session token from OAuth failed, result: {0}.", httpResponse.GetHttpContentString()));
            }

            string accessKeyId = GetString(map, "accessKeyId");
            string accessKeySecret = GetString(map, "accessKeySecret");
            string securityToken = GetString(map, "securityToken");
            string expirationStr = GetString(map, "expiration");

            if (string.IsNullOrEmpty(accessKeyId) || string.IsNullOrEmpty(accessKeySecret)
                || string.IsNullOrEmpty(securityToken))
            {
                throw new CredentialException(string.Format(
                    "Refresh session token from OAuth failed, fail to get credentials: {0}.",
                    httpResponse.GetHttpContentString()));
            }

            string cleanExpiration = expirationStr.Replace('T', ' ').Replace('Z', ' ');
            var dt = Convert.ToDateTime(cleanExpiration);
            long expiration = dt.GetTimeMillis();

            if (this.tokenUpdateCallback != null)
            {
                try
                {
                    this.tokenUpdateCallback(this.refreshToken, this.accessToken,
                        accessKeyId, accessKeySecret, securityToken,
                        this.accessTokenExpire, expiration / 1000);
                }
                catch (Exception)
                {
                    // Warning only
                }
            }

            CredentialModel credentialModel = new CredentialModel
            {
                AccessKeyId = accessKeyId,
                AccessKeySecret = accessKeySecret,
                SecurityToken = securityToken,
                Expiration = expiration,
                Type = AuthConstant.Sts,
                ProviderName = GetProviderName()
            };
            return new RefreshResult<CredentialModel>(credentialModel, GetStaleTime(expiration));
        }

        public override string GetProviderName()
        {
            return "oauth";
        }

        private static string GetString(Dictionary<string, object> values, string key)
        {
            if (values == null || !values.ContainsKey(key) || values[key] == null)
            {
                return null;
            }
            return values[key].ToString();
        }

        private static long GetUnixTimeSeconds()
        {
            return DateTime.UtcNow.GetTimeMillis() / 1000;
        }
    }
}
