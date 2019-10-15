using System;
using System.IO;

using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Http;
using Aliyun.Credentials.Utils;

using Newtonsoft.Json;

namespace Aliyun.Credentials.Provider
{
    public class EcsRamRoleCredentialProvider : IAlibabaCloudCredentialsProvider
    {
        private const string UrlInEcsMetadata = "/latest/meta-data/ram/security-credentials/";

        private const string EcsMetadatFetchErrorMsg =
            "Failed to get RAM session credentials from ECS metadata service.";

        private readonly string roleName;
        private string credentialUrl;
        private const string MetadataServiceHost = "100.100.100.200";
        private readonly int connectionTimeout = 1000;
        private readonly int readTimeout = 1000;

        public EcsRamRoleCredentialProvider(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentNullException("roleName", "You must specifiy a valid role name.");
            }

            this.roleName = roleName;
            SetCredentialUrl();
        }

        public EcsRamRoleCredentialProvider(Configuration config)
        {
            if (string.IsNullOrWhiteSpace(config.RoleName))
            {
                var e = new InvalidDataException("You must specify a valid role name.");
                throw new ArgumentNullException("You must specify a valid role name.", e);
            }

            if (config.ConnectTimeout > 1000)
            {
                this.connectionTimeout = config.ConnectTimeout;
            }

            if (config.ReadTimeout > 1000)
            {
                this.readTimeout = config.ReadTimeout;
            }

            roleName = config.RoleName;
            SetCredentialUrl();
        }

        private void SetCredentialUrl()
        {
            credentialUrl = "http://" + MetadataServiceHost + UrlInEcsMetadata + roleName;
        }

        public IAlibabaCloudCredentials GetCredentials()
        {
            CompatibleUrlConnClient client = new CompatibleUrlConnClient();
            return CreateCredential(client);
        }

        public IAlibabaCloudCredentials CreateCredential(IConnClient client)
        {
            return GetNewSessionCredentials(client);
        }

        private IAlibabaCloudCredentials GetNewSessionCredentials(IConnClient client)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.Method = MethodType.Get;
            httpRequest.ConnectTimeout = connectionTimeout;
            httpRequest.ReadTimeout = readTimeout;
            httpRequest.Url = credentialUrl;
            HttpResponse httpResponse;
            string jsonContent;
            string contentCode;
            string contentAccessKeyId;
            string contentAccessKeySecret;
            string contentSecurityToken;
            string contentExpiration;
            
            try
            {
                httpResponse = client.DoAction(httpRequest);
            }
            catch (Exception ex)
            {
                throw new CredentialException("Failed to connect ECS Metadata Service: " + ex.Message);
            }

            if (httpResponse != null && httpResponse.Status != 200)
            {
                throw new CredentialException(EcsMetadatFetchErrorMsg + " HttpCode=" + httpResponse.Status);
            }

            jsonContent = httpResponse.GetHttpContentString();
            dynamic contentObj = JsonConvert.DeserializeObject<dynamic>(jsonContent);
            try
            {
                contentCode = contentObj.Code;
                contentAccessKeyId = contentObj.AccessKeyId;
                contentAccessKeySecret = contentObj.AccessKeySecret;
                contentSecurityToken = contentObj.SecurityToken;
                contentExpiration = contentObj.Expiration;
            }
            catch
            {
                throw new CredentialException("Invalid json got from ECS Metadata service.");
            }

            if (contentCode != "Success")
            {
                throw new CredentialException(EcsMetadatFetchErrorMsg);
            }

            string expirationStr = contentExpiration.Replace('T', ' ').Replace('Z', ' ');
            var dt = Convert.ToDateTime(expirationStr);
            long expiration = dt.GetTimeMillis();
            return new EcsRamRoleCredential(contentAccessKeyId, contentAccessKeySecret, contentSecurityToken,
                expiration, this);
        }

        public string RoleName
        {
            get { return roleName; }
        }

        public string CredentialUrl
        {
            get { return credentialUrl; }
        }
    }
}
