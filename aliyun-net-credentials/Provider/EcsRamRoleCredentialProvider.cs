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
    public class EcsRamRoleCredentialProvider : SessionCredentialsProvider
    {
        private const string UrlInEcsMetadata = "/latest/meta-data/ram/security-credentials/";
        private const string UrlInMetadataToken = "/latest/api/token";
        private const string EcsMetadatFetchErrorMsg =
            "Failed to get RAM session credentials from ECS metadata service.";

        private string roleName;
        private string credentialUrl;
        private const string MetadataServiceHost = "100.100.100.200";
        private readonly int connectionTimeout = 1000;
        private readonly int readTimeout = 1000;
        private readonly bool disableIMDSv1;
        private const int metadataTokenDuration = 21600;

        public EcsRamRoleCredentialProvider(string roleName)
        {
            this.roleName = roleName;
            this.disableIMDSv1 = false;
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
            this.disableIMDSv1 = config.DisableIMDSv1 ?? AuthUtils.DisableIMDSv1;
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
            roleName = GetMetadata(client);
        }

        public async Task GetRoleNameAsync(IConnClient client)
        {
            roleName = await GetMetadataAsync(client);
        }

        private string GetMetadataToken(IConnClient client)
        {
            try
            {
                HttpRequest httpRequest = new HttpRequest("http://" + MetadataServiceHost + UrlInMetadataToken);
                httpRequest.Method = MethodType.PUT;
                httpRequest.ConnectTimeout = this.connectionTimeout;
                httpRequest.ReadTimeout = this.readTimeout;
                httpRequest.Headers.Add("X-aliyun-ecs-metadata-token-ttl-seconds", metadataTokenDuration.ToString());

                HttpResponse httpResponse;
                try
                {
                    httpResponse = client.DoAction(httpRequest);
                }
                catch (Exception ex)
                {
                    throw new CredentialException("Failed to connect ECS Metadata Service: " + ex.GetType() + ": " + ex.Message);
                }
                if (httpResponse != null && httpResponse.Status != 200)
                {
                    throw new CredentialException(EcsMetadatFetchErrorMsg + " HttpCode=" + httpResponse.Status + ", ResponseMessage=" + httpResponse.GetHttpContentString());
                }
                return httpResponse.GetHttpContentString();
            }
            catch (Exception ex)
            {
                if (this.disableIMDSv1)
                {
                    throw new CredentialException("Failed to get token from ECS Metadata Service, and fallback to IMDS v1 is disabled via the disableIMDSv1 configuration is turned on. Original error: " + ex.Message);
                }
                return null;
            }
        }

        private async Task<string> GetMetadataTokenAsync(IConnClient client)
        {
            try
            {
                HttpRequest httpRequest = new HttpRequest("http://" + MetadataServiceHost + UrlInMetadataToken);
                httpRequest.Method = MethodType.PUT;
                httpRequest.ConnectTimeout = this.connectionTimeout;
                httpRequest.ReadTimeout = this.readTimeout;
                httpRequest.Headers.Add("X-aliyun-ecs-metadata-token-ttl-seconds", metadataTokenDuration.ToString());

                HttpResponse httpResponse;
                try
                {
                    httpResponse = await client.DoActionAsync(httpRequest);
                }
                catch (Exception ex)
                {
                    throw new CredentialException("Failed to connect ECS Metadata Service: " + ex.GetType() + ": " + ex.Message);
                }
                if (httpResponse != null && httpResponse.Status != 200)
                {
                    throw new CredentialException(EcsMetadatFetchErrorMsg + " HttpCode=" + httpResponse.Status + ", ResponseMessage=" + httpResponse.GetHttpContentString());
                }
                return httpResponse.GetHttpContentString();
            }
            catch (Exception ex)
            {
                if (this.disableIMDSv1)
                {
                    throw new CredentialException("Failed to get token from ECS Metadata Service, and fallback to IMDS v1 is disabled via the disableIMDSv1 configuration is turned on. Original error: " + ex.Message);
                }
                return null;
            }
        }

        private string GetMetadata(IConnClient client)
        {
            HttpRequest httpRequest = new HttpRequest
            {
                Method = MethodType.GET,
                ConnectTimeout = connectionTimeout,
                ReadTimeout = readTimeout,
                Url = credentialUrl
            };

            string metadataToken = GetMetadataToken(client);
            if (metadataToken != null)
            {
                httpRequest.Headers.Add("X-aliyun-ecs-metadata-token", metadataToken);
            }

            HttpResponse httpResponse;
            try
            {
                httpResponse = client.DoAction(httpRequest);
            }
            catch (Exception ex)
            {
                throw new CredentialException("Failed to connect ECS Metadata Service: " + ex.GetType() + ": " + ex.Message);
            }

            if (httpResponse != null && httpResponse.Status == 404)
            {
                throw new CredentialException("The role name was not found in the instance");
            }

            if (httpResponse != null && httpResponse.Status != 200)
            {
                throw new CredentialException(EcsMetadatFetchErrorMsg + " HttpCode=" + httpResponse.Status);
            }
            return httpResponse.GetHttpContentString();
        }

        private async Task<string> GetMetadataAsync(IConnClient client)
        {
            HttpRequest httpRequest = new HttpRequest
            {
                Method = MethodType.GET,
                ConnectTimeout = connectionTimeout,
                ReadTimeout = readTimeout,
                Url = credentialUrl
            };

            string metadataToken = await GetMetadataTokenAsync(client);
            if (metadataToken != null)
            {
                httpRequest.Headers.Add("X-aliyun-ecs-metadata-token", metadataToken);
            }

            HttpResponse httpResponse;
            try
            {
                httpResponse = await client.DoActionAsync(httpRequest);
            }
            catch (Exception ex)
            {
                throw new CredentialException("Failed to connect ECS Metadata Service: " + ex.GetType() + ": " + ex.Message);
            }

            if (httpResponse != null && httpResponse.Status == 404)
            {
                throw new CredentialException("The role name was not found in the instance");
            }

            if (httpResponse != null && httpResponse.Status != 200)
            {
                throw new CredentialException(EcsMetadatFetchErrorMsg + " HttpCode=" + httpResponse.Status);
            }
            return httpResponse.GetHttpContentString();
        }

        private RefreshResult<CredentialModel> GetNewSessionCredentials(IConnClient client)
        {
            string jsonContent;
            string contentCode;
            string contentAccessKeyId;
            string contentAccessKeySecret;
            string contentSecurityToken;
            string contentExpiration;

            jsonContent = GetMetadata(client);
            Dictionary<string, string> contentObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent);
            try
            {
                contentCode = contentObj["Code"];
                contentAccessKeyId = contentObj["AccessKeyId"];
                contentAccessKeySecret = contentObj["AccessKeySecret"];
                contentSecurityToken = contentObj["SecurityToken"];
                contentExpiration = contentObj["Expiration"];
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
            string jsonContent;
            string contentCode;
            string contentAccessKeyId;
            string contentAccessKeySecret;
            string contentSecurityToken;
            string contentExpiration;

            jsonContent = await GetMetadataAsync(client);
            Dictionary<string, string> contentObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent);
            try
            {
                contentCode = contentObj["Code"];
                contentAccessKeyId = contentObj["AccessKeyId"];
                contentAccessKeySecret = contentObj["AccessKeySecret"];
                contentSecurityToken = contentObj["SecurityToken"];
                contentExpiration = contentObj["Expiration"];
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

        public bool DisableIMDSv1
        {
            get { return disableIMDSv1; }
        }
    }
}