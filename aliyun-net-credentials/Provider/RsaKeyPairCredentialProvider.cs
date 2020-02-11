using System;
using System.Diagnostics;

using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Http;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Utils;

using Newtonsoft.Json;

namespace Aliyun.Credentials.Provider
{
    public class RsaKeyPairCredentialProvider : IAlibabaCloudCredentialsProvider
    {
        private int durationSeconds = 3600;
        private string regionId = "cn-hangzhou";
        private int connectTimeout = 1000;
        private int readTimeout = 1000;

        public RsaKeyPairCredentialProvider(Config config) : this(config.PublicKeyId, config.PrivateKeyFile)
        {
            this.connectTimeout = config.ConnectTimeout;
            this.readTimeout = config.ReadTimeout;
        }

        public RsaKeyPairCredentialProvider(String publicKeyId, String privateKey)
        {
            PublicKeyId = publicKeyId;
            PrivateKey = privateKey;
        }

        public IAlibabaCloudCredentials GetCredentials()
        {
            CompatibleUrlConnClient client = new CompatibleUrlConnClient();
            return CreateCredential(client);
        }

        private IAlibabaCloudCredentials CreateCredential(IConnClient client)
        {
            return GetNewSessionCredentials(client);
        }

        private IAlibabaCloudCredentials GetNewSessionCredentials(IConnClient client)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.SetCommonUrlParameters();
            httpRequest.AddUrlParameter("Action", "GenerateSessionAccessKey");
            httpRequest.AddUrlParameter("Format", "JSON");
            httpRequest.AddUrlParameter("Version", "2015-04-01");
            httpRequest.AddUrlParameter("DurationSeconds", durationSeconds.ToString());
            httpRequest.AddUrlParameter("AccessKeyId", PublicKeyId);
            httpRequest.AddUrlParameter("RegionId", regionId);
            string strToSign = ParameterHelper.ComposeStringToSign(MethodType.Get, httpRequest.UrlParameters);
            String signature = ParameterHelper.SignString(strToSign, PrivateKey + "&");
            httpRequest.AddUrlParameter("Signature", signature);
            httpRequest.Method = MethodType.Get;
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
            return new RsaKeyPairCredential(sessionAccessKeyId, sessionAccessKeySecret, expiration, this);
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
