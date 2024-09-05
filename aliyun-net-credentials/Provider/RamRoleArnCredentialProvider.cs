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

        private string regionId = "cn-hangzhou";
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

        public IAlibabaCloudCredentialsProvider CredentialsProvider { get; set; }

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
            connectTimeout = config.ConnectTimeout;
            readTimeout = config.Timeout;
            policy = config.Policy;
            if (config.RoleSessionExpiration > 0)
            {
                durationSeconds = config.RoleSessionExpiration;
            }
            roleSessionName = config.RoleSessionName ?? Environment.GetEnvironmentVariable("ALIBABA_CLOUD_ROLE_SESSION_NAME") ?? roleSessionName;
            STSEndpoint = config.STSEndpoint ?? STSEndpoint;
        }

        public RamRoleArnCredentialProvider(string accessKeyId, string accessKeySecret, string roleArn)
        {
            CredentialsProvider = new StaticAKCredentialsProvider(accessKeyId, accessKeySecret);
            this.roleArn = roleArn ?? Environment.GetEnvironmentVariable("ALIBABA_CLOUD_ROLE_ARN");
        }

        public RamRoleArnCredentialProvider(IAlibabaCloudCredentialsProvider provider, string roleArn)
        {
            CredentialsProvider = ParameterHelper.ValidateNotNull(provider, "Provider", "Must specify a previous credentials provider to asssume role.");
            this.roleArn = roleArn ?? Environment.GetEnvironmentVariable("ALIBABA_CLOUD_ROLE_ARN");
        }

        public RamRoleArnCredentialProvider(IAlibabaCloudCredentialsProvider provider, string roleArn, int durationSeconds,
            string roleSessionName) : this(provider, roleArn)
        {
            this.roleSessionName = roleSessionName ?? Environment.GetEnvironmentVariable("ALIBABA_CLOUD_ROLE_SESSION_NAME") ?? this.roleSessionName;
            this.durationSeconds = durationSeconds;
        }

        public RamRoleArnCredentialProvider(string accessKeyId, string accessKeySecret, string roleSessionName,
            string roleArn, string regionId, string policy) : this(accessKeyId, accessKeySecret, roleArn)
        {
            this.roleSessionName = roleSessionName ?? Environment.GetEnvironmentVariable("ALIBABA_CLOUD_ROLE_SESSION_NAME") ?? this.roleSessionName;
            this.regionId = regionId;
            this.policy = policy;
        }

        public RamRoleArnCredentialProvider(IAlibabaCloudCredentialsProvider provider, string roleSessionName,
                    string roleArn, string regionId, string policy) : this(provider, roleArn)
        {
            this.roleSessionName = roleSessionName ?? Environment.GetEnvironmentVariable("ALIBABA_CLOUD_ROLE_SESSION_NAME") ?? this.roleSessionName;
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
            CredentialModel previousCredentials = CredentialsProvider.GetCredentials();
            ParameterHelper.ValidateNotNull(previousCredentials, "OriginalCredentials", "Unable to load original credentials from the providers in RAM role arn.");
            httpRequest.AddUrlParameter("AccessKeyId", previousCredentials.AccessKeyId);
            httpRequest.AddUrlParameter("SecurityToken", previousCredentials.SecurityToken);
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
            CredentialModel previousCredentials = await CredentialsProvider.GetCredentialsAsync();
            ParameterHelper.ValidateNotNull(previousCredentials, "OriginalCredentials", "Unable to load original credentials from the providers in RAM role arn.");
            httpRequest.AddUrlParameter("AccessKeyId", previousCredentials.AccessKeyId);
            httpRequest.AddUrlParameter("SecurityToken", previousCredentials.SecurityToken);
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
            string signature = ParameterHelper.SignString(strToSign, previousCredentials.AccessKeySecret + "&");
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
                CredentialModel credentialModel = new CredentialModel
                {
                    AccessKeyId = DictionaryUtil.Get(credentials, "AccessKeyId"),
                    AccessKeySecret = DictionaryUtil.Get(credentials, "AccessKeySecret"),
                    SecurityToken = DictionaryUtil.Get(credentials, "SecurityToken"),
                    Expiration = expiration,
                    Type = AuthConstant.RamRoleArn
                };
                return new RefreshResult<CredentialModel>(credentialModel, GetStaleTime(expiration));
            }

            throw new CredentialException(JsonConvert.SerializeObject(map));
        }
    }
}
