using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Http;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Utils;

using Newtonsoft.Json;

namespace Aliyun.Credentials.Provider
{
    public class RsaKeyPairCredentialProvider : SessionCredentialsProvider
    {
        private int durationSeconds = 3600;
        private string regionId = "cn-hangzhou";
        private int connectTimeout = 1000;
        private int readTimeout = 1000;

        public RsaKeyPairCredentialProvider(Config config) : this(config.PublicKeyId, config.PrivateKeyFile)
        {
            connectTimeout = config.ConnectTimeout;
            readTimeout = config.Timeout;
        }

        public RsaKeyPairCredentialProvider(string publicKeyId, string privateKey)
        {
            PublicKeyId = ParameterHelper.ValidateNotNull(publicKeyId, "publicKeyId", "PublicKeyId must not be null.");
            PrivateKey = ParameterHelper.ValidateNotNull(privateKey, "privateKey", "PrivateKeyFile must not be null.");
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
            httpRequest.AddUrlParameter("Action", "GenerateSessionAccessKey");
            httpRequest.AddUrlParameter("Format", "JSON");
            httpRequest.AddUrlParameter("Version", "2015-04-01");
            httpRequest.AddUrlParameter("DurationSeconds", durationSeconds.ToString());
            httpRequest.AddUrlParameter("AccessKeyId", PublicKeyId);
            httpRequest.AddUrlParameter("RegionId", regionId);
            string strToSign = ParameterHelper.ComposeStringToSign(MethodType.GET, httpRequest.UrlParameters);
            string signature = ParameterHelper.SignString(strToSign, PrivateKey + "&");
            httpRequest.AddUrlParameter("Signature", signature);
            httpRequest.Method = MethodType.GET;
            httpRequest.ConnectTimeout = connectTimeout;
            httpRequest.ReadTimeout = readTimeout;
            httpRequest.Url = ParameterHelper.ComposeUrl("sts.aliyuncs.com", httpRequest.UrlParameters, "https");
            HttpResponse httpResponse = client.DoAction(httpRequest);
            if (httpResponse != null && httpResponse.Status != 200)
            {
                throw new CredentialException("Failed to get session credentials.HttpCode=" + httpResponse.Status);
            }

            Debug.Assert(httpResponse != null, "httpResponse != null");
            dynamic contentObj = JsonConvert.DeserializeObject<dynamic>(httpResponse.GetHttpContentString());
            string sessionAccessKeyId;
            string sessionAccessKeySecret;
            string expirationStr;
            try
            {
                sessionAccessKeyId = contentObj.SessionAccessKey.SessionAccessKeyId;
                sessionAccessKeySecret = contentObj.SessionAccessKey.SessionAccessKeySecret;
                expirationStr = contentObj.SessionAccessKey.Expiration;
            }
            catch
            {
                throw new CredentialException("Invalid json got from service.");
            }
            expirationStr = expirationStr.Replace('T', ' ').Replace('Z', ' ');
            var dt = Convert.ToDateTime(expirationStr);
            long expiration = dt.GetTimeMillis();
            CredentialModel credentialModel = new CredentialModel
            {
                AccessKeyId = sessionAccessKeyId,
                AccessKeySecret = sessionAccessKeySecret,
                Expiration = expiration,
                Type = AuthConstant.RsaKeyPair
            };
            return new RefreshResult<CredentialModel>(credentialModel, GetStaleTime(expiration));
        }

        private async Task<RefreshResult<CredentialModel>> GetNewSessionCredentialsAsync(IConnClient client)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.SetCommonUrlParameters();
            httpRequest.AddUrlParameter("Action", "GenerateSessionAccessKey");
            httpRequest.AddUrlParameter("Format", "JSON");
            httpRequest.AddUrlParameter("Version", "2015-04-01");
            httpRequest.AddUrlParameter("DurationSeconds", durationSeconds.ToString());
            httpRequest.AddUrlParameter("AccessKeyId", PublicKeyId);
            httpRequest.AddUrlParameter("RegionId", regionId);
            string strToSign = ParameterHelper.ComposeStringToSign(MethodType.GET, httpRequest.UrlParameters);
            String signature = ParameterHelper.SignString(strToSign, PrivateKey + "&");
            httpRequest.AddUrlParameter("Signature", signature);
            httpRequest.Method = MethodType.GET;
            httpRequest.ConnectTimeout = connectTimeout;
            httpRequest.ReadTimeout = readTimeout;
            httpRequest.Url = ParameterHelper.ComposeUrl("sts.aliyuncs.com", httpRequest.UrlParameters, "https");
            HttpResponse httpResponse = await client.DoActionAsync(httpRequest);
            if (httpResponse != null && httpResponse.Status != 200)
            {
                throw new CredentialException("Failed to get session credentials.HttpCode=" + httpResponse.Status);
            }

            Debug.Assert(httpResponse != null, "httpResponse != null");
            dynamic contentObj = JsonConvert.DeserializeObject<dynamic>(httpResponse.GetHttpContentString());
            string sessionAccessKeyId;
            string sessionAccessKeySecret;
            string expirationStr;
            try
            {
                sessionAccessKeyId = contentObj.SessionAccessKey.SessionAccessKeyId;
                sessionAccessKeySecret = contentObj.SessionAccessKey.SessionAccessKeySecret;
                expirationStr = contentObj.SessionAccessKey.Expiration;
            }
            catch
            {
                throw new CredentialException("Invalid json got from service.");
            }
            expirationStr = expirationStr.Replace('T', ' ').Replace('Z', ' ');
            var dt = Convert.ToDateTime(expirationStr);
            long expiration = dt.GetTimeMillis();
            CredentialModel credentialModel = new CredentialModel
            {
                AccessKeyId = sessionAccessKeyId,
                AccessKeySecret = sessionAccessKeySecret,
                Expiration = expiration,
                Type = AuthConstant.RsaKeyPair
            };
            return new RefreshResult<CredentialModel>(credentialModel, GetStaleTime(expiration));
        }

        public int DurationSeconds
        {
            get
            {
                return durationSeconds;
            }

            set
            {
                durationSeconds = value;
            }
        }

        public string PublicKeyId { get; set; }

        public string PrivateKey { get; set; }

        public string RegionId
        {
            get
            {
                return regionId;
            }

            set
            {
                regionId = value;
            }
        }

        public int ConnectTimeout
        {
            get
            {
                return connectTimeout;
            }

            set
            {
                connectTimeout = value;
            }
        }

        public int ReadTimeout
        {
            get
            {
                return readTimeout;
            }

            set
            {
                readTimeout = value;
            }
        }
    }
}
