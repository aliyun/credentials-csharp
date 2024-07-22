using System;
using System.Threading.Tasks;

using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Http;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Utils;

using Newtonsoft.Json;

namespace Aliyun.Credentials.Provider
{
    public class EcsRamRoleCredentialProvider : SessionCredentialsProvider
    {
        private const string UrlInEcsMetadata = "/latest/meta-data/ram/security-credentials/";

        private const string EcsMetadatFetchErrorMsg =
            "Failed to get RAM session credentials from ECS metadata service.";

        private string roleName;
        private string credentialUrl;
        private const string MetadataServiceHost = "100.100.100.200";
        private readonly int connectionTimeout = 1000;
        private readonly int readTimeout = 1000;

        public EcsRamRoleCredentialProvider(string roleName)
        {
            this.roleName = roleName;
            SetCredentialUrl();
        }

        public EcsRamRoleCredentialProvider(Config config)
        {
            if (config.ConnectTimeout > 1000)
            {
                connectionTimeout = config.ConnectTimeout;
            }

            if (config.Timeout > 1000)
            {
                readTimeout = config.Timeout;
            }

            roleName = config.RoleName;
            SetCredentialUrl();
        }

        private void SetCredentialUrl()
        {
            credentialUrl = "http://" + MetadataServiceHost + UrlInEcsMetadata + roleName;
        }

        public override RefreshResult<CredentialModel> RefreshCredentials()
        {
            CompatibleUrlConnClient client = new CompatibleUrlConnClient();
            if (string.IsNullOrWhiteSpace(roleName))
            {
                GetRoleName(client);
                SetCredentialUrl();
            }
            return CreateCredential(client);
        }

        public override async Task<RefreshResult<CredentialModel>> RefreshCredentialsAsync()
        {
            CompatibleUrlConnClient client = new CompatibleUrlConnClient();
            if (string.IsNullOrWhiteSpace(roleName))
            {
                await GetRoleNameAsync(client);
                SetCredentialUrl();
            }
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

        public void GetRoleName(IConnClient client)
        {
            HttpRequest httpRequest = new HttpRequest
            {
                Method = MethodType.GET,
                ConnectTimeout = connectionTimeout,
                ReadTimeout = readTimeout,
                Url = credentialUrl
            };
            HttpResponse httpResponse;

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

            roleName = httpResponse.GetHttpContentString();
        }

        public async Task GetRoleNameAsync(IConnClient client)
        {
            HttpRequest httpRequest = new HttpRequest
            {
                Method = MethodType.GET,
                ConnectTimeout = connectionTimeout,
                ReadTimeout = readTimeout,
                Url = credentialUrl
            };
            HttpResponse httpResponse;

            try
            {
                httpResponse = await client.DoActionAsync(httpRequest);
            }
            catch (Exception ex)
            {
                throw new CredentialException("Failed to connect ECS Metadata Service: " + ex.Message);
            }

            if (httpResponse != null && httpResponse.Status != 200)
            {
                throw new CredentialException(EcsMetadatFetchErrorMsg + " HttpCode=" + httpResponse.Status);
            }

            roleName = httpResponse.GetHttpContentString();
        }

        private RefreshResult<CredentialModel> GetNewSessionCredentials(IConnClient client)
        {
            HttpRequest httpRequest = new HttpRequest
            {
                Method = MethodType.GET,
                ConnectTimeout = connectionTimeout,
                ReadTimeout = readTimeout,
                Url = credentialUrl
            };
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
            CredentialModel credentialModel = new CredentialModel
            {
                AccessKeyId = contentAccessKeyId,
                AccessKeySecret = contentAccessKeySecret,
                SecurityToken = contentSecurityToken,
                Expiration = expiration,
                Type = AuthConstant.EcsRamRole
            };
            return new RefreshResult<CredentialModel>(credentialModel, GetStaleTime(expiration));
        }

        private async Task<RefreshResult<CredentialModel>> GetNewSessionCredentialsAsync(IConnClient client)
        {
            HttpRequest httpRequest = new HttpRequest
            {
                Method = MethodType.GET,
                ConnectTimeout = connectionTimeout,
                ReadTimeout = readTimeout,
                Url = credentialUrl
            };
            HttpResponse httpResponse;
            string jsonContent;
            string contentCode;
            string contentAccessKeyId;
            string contentAccessKeySecret;
            string contentSecurityToken;
            string contentExpiration;

            try
            {
                httpResponse = await client.DoActionAsync(httpRequest);
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
            CredentialModel credentialModel = new CredentialModel
            {
                AccessKeyId = contentAccessKeyId,
                AccessKeySecret = contentAccessKeySecret,
                SecurityToken = contentSecurityToken,
                Expiration = expiration,
                Type = AuthConstant.EcsRamRole
            };
            return new RefreshResult<CredentialModel>(credentialModel, GetStaleTime(expiration));
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
