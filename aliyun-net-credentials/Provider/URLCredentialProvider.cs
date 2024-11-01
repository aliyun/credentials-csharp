using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Http;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Utils;

using Newtonsoft.Json;
using Tea.Utils;

namespace Aliyun.Credentials.Provider
{
    public class URLCredentialProvider : SessionCredentialsProvider
    {
        private readonly Uri credentialsURI;

        /// <summary>
        /// Unit of millsecond
        /// </summary>
        private int connectTimeout = 1000;
        private int readTimeout = 1000;

        [Obsolete("Use builder instead.")]
        public URLCredentialProvider(Config config) : this(config.CredentialsURI)
        {
            connectTimeout = config.ConnectTimeout;
            readTimeout = config.Timeout;
        }

        [Obsolete("Use builder instead.")]
        public URLCredentialProvider(string credentialsURI)
        {
            try
            {
                string uriStr = credentialsURI ?? Environment.GetEnvironmentVariable("ALIBABA_CLOUD_CREDENTIALS_URI");
                this.credentialsURI = new Uri(ParameterHelper.ValidateNotNull(credentialsURI, "credentialsURI", "Credentials URI is not valid."));

            }
            catch (UriFormatException)
            {
                throw new CredentialException("Credential URI is not valid.");
            }
        }

        private URLCredentialProvider(Builder builder)
        {
            this.connectTimeout = builder.connectionTimeout;
            this.readTimeout = builder.readTimeout;
            try
            {
                string uriStr = builder.credentialsURI ?? Environment.GetEnvironmentVariable("ALIBABA_CLOUD_CREDENTIALS_URI");
                this.credentialsURI = new Uri(ParameterHelper.ValidateNotNull(builder.credentialsURI, "credentialsURI", "Credentials URI is not valid."));

            }
            catch (UriFormatException)
            {
                throw new CredentialException("Credential URI is not valid.");
            }
        }

        public class Builder
        {
            internal string credentialsURI = Environment.GetEnvironmentVariable("ALIBABA_CLOUD_CREDENTIALS_URI");
            internal int connectionTimeout = 1000;
            internal int readTimeout = 1000;

            public Builder CredentialsURI(string credentialsURI)
            {
                this.credentialsURI = credentialsURI;
                return this;
            }

            public Builder CredentialsURI(Uri credentialsURI)
            {
                this.credentialsURI = credentialsURI.ToString();
                return this;
            }

            public Builder ConnectionTimeout(int connectionTimeout)
            {
                this.connectionTimeout = connectionTimeout > 0 ? connectionTimeout : 1000;
                return this;
            }

            public Builder ReadTimeout(int readTimeout)
            {
                this.readTimeout = readTimeout > 0 ? readTimeout : 1000;
                return this;
            }

            public URLCredentialProvider Build()
            {
                return new URLCredentialProvider(this);
            }
        }

        public override string GetProviderName()
        {
            return "credentials_uri";
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
            HttpRequest httpRequest = new HttpRequest(credentialsURI.ToString());
            httpRequest.SetCommonUrlParameters();
            httpRequest.Method = MethodType.GET;
            httpRequest.ConnectTimeout = connectTimeout;
            httpRequest.ReadTimeout = readTimeout;
            HttpResponse httpResponse;

            try
            {
                httpResponse = client.DoAction(httpRequest);
            }
            catch (Exception e)
            {
                throw new CredentialException("Failed to connect Server: " + credentialsURI.ToString() + e.ToString());
            }

            if (httpResponse.Status >= 300 || httpResponse.Status < 200)
            {
                throw new CredentialException("Failed to get credentials from server: " + credentialsURI.ToString() +
                "\nHttpCode=" + httpResponse.Status +
                "\nHttpRAWContent=" + httpResponse.GetHttpContentString());
            }

            Dictionary<string, string> map =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(httpResponse.GetHttpContentString());
            if (map.ContainsKey("Code") && DictionaryUtil.Get(map, "Code") == "Success")
            {
                string expirationStr = DictionaryUtil.Get(map, "Expiration").Replace('T', ' ').Replace('Z', ' ');
                var dt = Convert.ToDateTime(expirationStr);
                long expiration = dt.GetTimeMillis();
                CredentialModel credentialModel = new CredentialModel
                {
                    Expiration = expiration,
                    Type = AuthConstant.CredentialsURI,
                    ProviderName = GetProviderName()
                };
                return new RefreshResult<CredentialModel>(credentialModel, GetStaleTime(expiration));
            }

            throw new CredentialException(JsonConvert.SerializeObject(map));
        }

        private async Task<RefreshResult<CredentialModel>> GetNewSessionCredentialsAsync(IConnClient client)
        {
            HttpRequest httpRequest = new HttpRequest(credentialsURI.ToString());
            httpRequest.SetCommonUrlParameters();
            httpRequest.Method = MethodType.GET;
            httpRequest.ConnectTimeout = connectTimeout;
            httpRequest.ReadTimeout = readTimeout;
            HttpResponse httpResponse;

            try
            {
                httpResponse = await client.DoActionAsync(httpRequest);
            }
            catch (Exception e)
            {
                throw new CredentialException("Failed to connect Server: " + credentialsURI.ToString() + e.ToString());
            }

            if (httpResponse.Status >= 300 || httpResponse.Status < 200)
            {
                throw new CredentialException("Failed to get credentials from server: " + credentialsURI.ToString() +
                "\nHttpCode=" + httpResponse.Status +
                "\nHttpRAWContent=" + httpResponse.GetHttpContentString());
            }

            Dictionary<string, string> map =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(httpResponse.GetHttpContentString());
            if (map.ContainsKey("Code") && DictionaryUtil.Get(map, "Code") == "Success")
            {
                string expirationStr = DictionaryUtil.Get(map, "Expiration").Replace('T', ' ').Replace('Z', ' ');
                var dt = Convert.ToDateTime(expirationStr);
                long expiration = dt.GetTimeMillis();
                CredentialModel credentialModel = new CredentialModel
                {
                    Expiration = expiration,
                    Type = AuthConstant.CredentialsURI,
                    ProviderName = GetProviderName()
                };
                return new RefreshResult<CredentialModel>(credentialModel, GetStaleTime(expiration));
            }

            throw new CredentialException(JsonConvert.SerializeObject(map));
        }
    }
}