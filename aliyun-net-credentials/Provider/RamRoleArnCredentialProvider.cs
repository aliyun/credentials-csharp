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
        private string roleSessionName = "defaultSessionName";

        private string accessKeyId;
        private string accessKeySecret;
        private string securityToken;
        private string regionId = "cn-hangzhou";
        private string policy;

        /// <summary>
        /// Unit of millisecond
        /// </summary>
        private int connectTimeout = 1000;

        private int readTimeout = 1000;

        public RamRoleArnCredentialProvider(Config config) : this(config.AccessKeyId, config.AccessKeySecret,
            config.RoleArn)
        {
            connectTimeout = config.ConnectTimeout;
            readTimeout = config.Timeout;
            policy = config.Policy;
            if (config.RoleSessionExpiration > 0)
            {
                durationSeconds = config.RoleSessionExpiration;
            }
        }

        public RamRoleArnCredentialProvider(string accessKeyId, string accessKeySecret, string roleArn)
        {
            this.roleArn = roleArn;
            this.accessKeyId = ParameterHelper.ValidateNotNull(accessKeyId, "accessKeyI", "AccessKeyId must not be null.");
            this.accessKeySecret = ParameterHelper.ValidateNotNull(accessKeySecret, "accessKeySecret", "AccessKeySecret must not be null.");
        }

        public RamRoleArnCredentialProvider(string accessKeyId, string accessKeySecret, string roleSessionName,
            string roleArn, string regionId, string policy) : this(accessKeyId, accessKeySecret, roleArn)
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
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.SetCommonUrlParameters();
            httpRequest.AddUrlParameter("Action", "AssumeRole");
            httpRequest.AddUrlParameter("Format", "JSON");
            httpRequest.AddUrlParameter("Version", "2015-04-01");
            httpRequest.AddUrlParameter("DurationSeconds", durationSeconds.ToString());
            httpRequest.AddUrlParameter("RoleArn", this.roleArn);
            httpRequest.AddUrlParameter("AccessKeyId", this.accessKeyId);
            httpRequest.AddUrlParameter("RegionId", this.regionId);
            httpRequest.AddUrlParameter("RoleSessionName", this.roleSessionName);
            if (policy != null)
            {
                httpRequest.AddUrlParameter("Policy", this.policy);
            }

            httpRequest.Method = MethodType.GET;
            httpRequest.ConnectTimeout = connectTimeout;
            httpRequest.ReadTimeout = readTimeout;
            string strToSign = ParameterHelper.ComposeStringToSign(MethodType.GET, httpRequest.UrlParameters);
            string signature = ParameterHelper.SignString(strToSign, accessKeySecret + "&");
            httpRequest.AddUrlParameter("Signature", signature);
            httpRequest.Url = ParameterHelper.ComposeUrl("sts.aliyuncs.com", httpRequest.UrlParameters,
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
                accessKeyId = DictionaryUtil.Get(credentials, "AccessKeyId");
                accessKeySecret = DictionaryUtil.Get(credentials, "AccessKeySecret");
                securityToken = DictionaryUtil.Get(credentials, "SecurityToken");
                CredentialModel credentialModel = new CredentialModel
                {
                    AccessKeyId = accessKeyId,
                    AccessKeySecret = accessKeySecret,
                    SecurityToken = securityToken,
                    Expiration = expiration,
                    Type = AuthConstant.RamRoleArn
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
            httpRequest.AddUrlParameter("AccessKeyId", this.accessKeyId);
            httpRequest.AddUrlParameter("RegionId", this.regionId);
            httpRequest.AddUrlParameter("RoleSessionName", this.roleSessionName);
            if (policy != null)
            {
                httpRequest.AddUrlParameter("Policy", this.policy);
            }

            httpRequest.Method = MethodType.GET;
            httpRequest.ConnectTimeout = connectTimeout;
            httpRequest.ReadTimeout = readTimeout;
            string strToSign = ParameterHelper.ComposeStringToSign(MethodType.GET, httpRequest.UrlParameters);
            string signature = ParameterHelper.SignString(strToSign, accessKeySecret + "&");
            httpRequest.AddUrlParameter("Signature", signature);
            httpRequest.Url = ParameterHelper.ComposeUrl("sts.aliyuncs.com", httpRequest.UrlParameters,
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
                accessKeyId = DictionaryUtil.Get(credentials, "AccessKeyId");
                accessKeySecret = DictionaryUtil.Get(credentials, "AccessKeySecret");
                securityToken = DictionaryUtil.Get(credentials, "SecurityToken");
                CredentialModel credentialModel = new CredentialModel
                {
                    AccessKeyId = accessKeyId,
                    AccessKeySecret = accessKeySecret,
                    SecurityToken = securityToken,
                    Expiration = expiration,
                    Type = AuthConstant.RamRoleArn
                };
                return new RefreshResult<CredentialModel>(credentialModel, GetStaleTime(expiration));
            }

            throw new CredentialException(JsonConvert.SerializeObject(map));
        }
    }
}
