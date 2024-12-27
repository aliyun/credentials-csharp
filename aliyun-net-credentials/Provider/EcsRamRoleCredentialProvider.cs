using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Http;
using Aliyun.Credentials.Logging;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Policy;
using Aliyun.Credentials.Utils;
using Newtonsoft.Json;
using Tea.Utils;

namespace Aliyun.Credentials.Provider
{
    /// <summary>
    /// Both ECS and ECI instances support binding instance RAM roles. When you use the Credentials tool in an instance, you will automatically obtain the RAM role bound to the instance, and obtain the STS Token of the RAM role by accessing the metadata service to complete the initialization of the credential client.
    /// </summary>
    public class EcsRamRoleCredentialProvider : SessionCredentialsProvider, IDisposable
    {
        private static readonly ILog Logger = LogProvider.For<EcsRamRoleCredentialProvider>();

        private const int AsyncRefreshIntervalTimeMinutes = 1;
        private volatile bool shouldRefresh;

        private const string UrlInEcsMetadata = "/latest/meta-data/ram/security-credentials/";
        private const string UrlInMetadataToken = "/latest/api/token";

        private const string EcsMetadataFetchErrorMsg =
            "Failed to get RAM session credentials from ECS metadata service.";

        private string roleName;
        private string credentialUrl;
        private const string MetadataServiceHost = "100.100.100.200";
        private readonly int connectTimeout = 1000;
        private readonly int readTimeout = 1000;
        private readonly bool disableIMDSv1;
        private const int metadataTokenDuration = 21600;

        private Timer _updateTimer;

        [Obsolete("Use builder instead.")]
        public EcsRamRoleCredentialProvider(string roleName)
        {
            this.roleName = roleName;
            this.disableIMDSv1 = false;
            SetCredentialUrl();
            CheckCredentialsUpdateAsynchronously();
        }

        [Obsolete("Use builder instead.")]
        public EcsRamRoleCredentialProvider(Config config)
        {
            if (config.ConnectTimeout > 1000)
            {
                connectTimeout = config.ConnectTimeout;
            }

            if (config.Timeout > 1000)
            {
                readTimeout = config.Timeout;
            }

            this.disableIMDSv1 = config.DisableIMDSv1 ?? AuthUtils.DisableIMDSv1;
            roleName = config.RoleName;
            SetCredentialUrl();
            CheckCredentialsUpdateAsynchronously();
        }

        private void CheckCredentialsUpdateAsynchronously()
        {
            if (!IsAsyncCredentialUpdateEnabled()) return;
            this._updateTimer = new Timer(AsyncRefreshIntervalTimeMinutes * 60 * 1000);
            this._updateTimer.Elapsed += (sender, e) => UpdateCredentials();
            this._updateTimer.Start();
        }

        private void UpdateCredentials()
        {
            try
            {
                if (!this.shouldRefresh) return;
                Logger.Info("Begin checking or refreshing credentials asynchronously");
                // 这里使用同步方法来刷新，因为 Timer.Elapsed 事件的处理程序是在一个线程池线程而非主线程上执行
                GetCredentials();
            }
            catch (Exception ex)
            {
                Logger.Warn(string.Format("Failed when checking or refreshing credentials asynchronously, error: {0}.",
                    ex.Message));
            }
        }

        public void Dispose()
        {
            if (this._updateTimer != null) this._updateTimer.Stop();
            if (this._updateTimer != null) this._updateTimer.Dispose();
        }

        private EcsRamRoleCredentialProvider(Builder builder) : base(builder)
        {
            var metadataDisabled = AuthUtils.EnvironmentEcsMetaDataDisabled ?? "";
            if (metadataDisabled.ToLower() == "true")
            {
                throw new CredentialException("IMDS credentials is disabled");
            }

            this.roleName = builder.roleName;
            this.disableIMDSv1 = builder.disableIMDSv1 ?? AuthUtils.DisableIMDSv1;
            this.connectTimeout = (builder.connectTimeout == null || builder.connectTimeout <= 0)
                ? 5000
                : builder.connectTimeout.Value;
            this.readTimeout = (builder.readTimeout == null || builder.readTimeout <= 0)
                ? 10000
                : builder.readTimeout.Value;
            SetCredentialUrl();
            CheckCredentialsUpdateAsynchronously();
        }

        public new class Builder : SessionCredentialsProvider.Builder
        {
            internal string roleName;
            internal bool? disableIMDSv1;
            internal int? connectTimeout;
            internal int? readTimeout;

            public Builder()
            {
                this.asyncCredentialUpdateEnabled = true;
                jitterEnabled = true;
                staleValueBehavior = Policy.StaleValueBehavior.Allow;
            }

            public Builder RoleName(string roleName)
            {
                this.roleName = roleName;
                return this;
            }

            public Builder DisableIMDSv1(bool? disableIMDSv1)
            {
                this.disableIMDSv1 = disableIMDSv1;
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

            public Builder AsyncCredentialUpdateEnabled(bool asyncCredentialUpdateEnabledInBuilder)
            {
                this.asyncCredentialUpdateEnabled = asyncCredentialUpdateEnabledInBuilder;
                return this;
            }

            public Builder JitterEnabled(bool jitterEnabledInBuilder)
            {
                this.jitterEnabled = jitterEnabledInBuilder;
                return this;
            }

            public Builder StaleValueBehavior(StaleValueBehavior staleValueBehavior)
            {
                this.staleValueBehavior = staleValueBehavior;
                return this;
            }

            public EcsRamRoleCredentialProvider Build()
            {
                this.asyncCredentialUpdateEnabled = true;
                return new EcsRamRoleCredentialProvider(this);
            }
        }

        private void SetCredentialUrl()
        {
            credentialUrl = "http://" + MetadataServiceHost + UrlInEcsMetadata + roleName;
        }

        public override RefreshResult<CredentialModel> RefreshCredentials()
        {
            var client = new CompatibleUrlConnClient();
            this.shouldRefresh = true;
            return CreateCredential(client);
        }

        public override async Task<RefreshResult<CredentialModel>> RefreshCredentialsAsync()
        {
            var client = new CompatibleUrlConnClient();
            this.shouldRefresh = true;
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

        private string GetMetadata(IConnClient client)
        {
            return GetMetadata(client, credentialUrl);
        }

        private string GetMetadata(IConnClient client, string url)
        {
            HttpRequest httpRequest = new HttpRequest
            {
                Method = MethodType.GET,
                ConnectTimeout = connectTimeout,
                ReadTimeout = readTimeout,
                Url = url
            };

            var metadataToken = GetMetadataToken(client);
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
                throw new CredentialException("Failed to connect ECS Metadata Service: " + ex.GetType() + ": " +
                                              ex.Message);
            }

            if (httpResponse != null && httpResponse.Status == 404)
            {
                throw new CredentialException("The role name was not found in the instance");
            }

            if (httpResponse != null && httpResponse.Status != 200)
            {
                throw new CredentialException(EcsMetadataFetchErrorMsg + " HttpCode=" + httpResponse.Status);
            }

            return httpResponse.GetHttpContentString();
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
                httpRequest.ConnectTimeout = this.connectTimeout;
                httpRequest.ReadTimeout = this.readTimeout;
                httpRequest.Headers.Add("X-aliyun-ecs-metadata-token-ttl-seconds", metadataTokenDuration.ToString());

                HttpResponse httpResponse;
                try
                {
                    httpResponse = client.DoAction(httpRequest);
                }
                catch (Exception ex)
                {
                    throw new CredentialException("Failed to connect ECS Metadata Service: " + ex.GetType() + ": " +
                                                  ex.Message);
                }

                if (httpResponse != null && httpResponse.Status != 200)
                {
                    throw new CredentialException(EcsMetadataFetchErrorMsg + " HttpCode=" + httpResponse.Status +
                                                  ", ResponseMessage=" + httpResponse.GetHttpContentString());
                }

                return httpResponse.GetHttpContentString();
            }
            catch (Exception ex)
            {
                if (this.disableIMDSv1)
                {
                    throw new CredentialException(
                        "Failed to get token from ECS Metadata Service, and fallback to IMDS v1 is disabled via the disableIMDSv1 configuration is turned on. Original error: " +
                        ex.Message);
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
                httpRequest.ConnectTimeout = this.connectTimeout;
                httpRequest.ReadTimeout = this.readTimeout;
                httpRequest.Headers.Add("X-aliyun-ecs-metadata-token-ttl-seconds", metadataTokenDuration.ToString());

                HttpResponse httpResponse;
                try
                {
                    httpResponse = await client.DoActionAsync(httpRequest);
                }
                catch (Exception ex)
                {
                    throw new CredentialException("Failed to connect ECS Metadata Service: " + ex.GetType() + ": " +
                                                  ex.Message);
                }

                if (httpResponse != null && httpResponse.Status != 200)
                {
                    throw new CredentialException(EcsMetadataFetchErrorMsg + " HttpCode=" + httpResponse.Status +
                                                  ", ResponseMessage=" + httpResponse.GetHttpContentString());
                }

                if (httpResponse != null) return httpResponse.GetHttpContentString();

                throw new CredentialException("Http response is null");
            }
            catch (Exception ex)
            {
                if (this.disableIMDSv1)
                {
                    throw new CredentialException(
                        "Failed to get token from ECS Metadata Service, and fallback to IMDS v1 is disabled via the disableIMDSv1 configuration is turned on. Original error: " +
                        ex.Message);
                }

                return null;
            }
        }

        private async Task<string> GetMetadataAsync(IConnClient client)
        {
            return await GetMetadataAsync(client, this.credentialUrl);
        }

        private async Task<string> GetMetadataAsync(IConnClient client, string url)
        {
            HttpRequest httpRequest = new HttpRequest
            {
                Method = MethodType.GET,
                ConnectTimeout = connectTimeout,
                ReadTimeout = readTimeout,
                Url = url
            };

            var metadataToken = await GetMetadataTokenAsync(client);
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
                throw new CredentialException("Failed to connect ECS Metadata Service: " + ex.GetType() + ": " +
                                              ex.Message);
            }

            if (httpResponse != null && httpResponse.Status == 404)
            {
                throw new CredentialException("The role name was not found in the instance");
            }

            if (httpResponse != null && httpResponse.Status != 200)
            {
                throw new CredentialException(EcsMetadataFetchErrorMsg + " HttpCode=" + httpResponse.Status);
            }

            if (httpResponse != null) return httpResponse.GetHttpContentString();

            throw new CredentialException("Http response is null");
        }

        private RefreshResult<CredentialModel> GetNewSessionCredentials(IConnClient client)
        {
            string contentAccessKeyId;
            string contentAccessKeySecret;
            string contentSecurityToken;
            string contentExpiration;

            var currentRoleName = this.roleName;
            if (string.IsNullOrWhiteSpace(this.roleName))
            {
                currentRoleName = GetMetadata(client, "http://" + MetadataServiceHost + UrlInEcsMetadata);
            }

            var jsonContent = GetMetadata(client, "http://" + MetadataServiceHost + UrlInEcsMetadata + currentRoleName);
            var contentObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent);

            if (!"Success".Equals(contentObj.Get("Code")))
            {
                throw new CredentialException(EcsMetadataFetchErrorMsg);
            }

            if (!contentObj.ContainsKey("AccessKeyId") || !contentObj.ContainsKey("AccessKeySecret") ||
                !contentObj.ContainsKey("SecurityToken"))
            {
                throw new CredentialException(string.Format("Error retrieving credentials from IMDS result: {0}.",
                    jsonContent));
            }

            try
            {
                contentAccessKeyId = contentObj["AccessKeyId"];
                contentAccessKeySecret = contentObj["AccessKeySecret"];
                contentSecurityToken = contentObj["SecurityToken"];
                contentExpiration = contentObj["Expiration"];
            }
            catch
            {
                throw new CredentialException("Invalid json got from ECS Metadata service.");
            }

            var expirationStr = contentExpiration.Replace('T', ' ').Replace('Z', ' ');
            var dt = Convert.ToDateTime(expirationStr);
            var expiration = dt.GetTimeMillis();
            var credentialModel = new CredentialModel
            {
                AccessKeyId = contentAccessKeyId,
                AccessKeySecret = contentAccessKeySecret,
                SecurityToken = contentSecurityToken,
                Expiration = expiration,
                Type = AuthConstant.EcsRamRole,
                ProviderName = GetProviderName()
            };
            return new RefreshResult<CredentialModel>.Builder(credentialModel).StaleTime(GetStaleTime(expiration))
                .PrefetchTime(GetPrefetchTime(expiration)).Build();
        }

        private async Task<RefreshResult<CredentialModel>> GetNewSessionCredentialsAsync(IConnClient client)
        {
            string contentCode;
            string contentAccessKeyId;
            string contentAccessKeySecret;
            string contentSecurityToken;
            string contentExpiration;

            // var jsonContent = await GetMetadataAsync(client);
            var currentRoleName = this.roleName;
            if (string.IsNullOrWhiteSpace(this.roleName))
            {
                currentRoleName = await GetMetadataAsync(client, "http://" + MetadataServiceHost + UrlInEcsMetadata);
            }

            var jsonContent = await GetMetadataAsync(client,
                "http://" + MetadataServiceHost + UrlInEcsMetadata + currentRoleName);

            var contentObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent);

            if (!"Success".Equals(contentObj.Get("Code")))
            {
                throw new CredentialException(EcsMetadataFetchErrorMsg);
            }

            if (!contentObj.ContainsKey("AccessKeyId") || !contentObj.ContainsKey("AccessKeySecret") ||
                !contentObj.ContainsKey("SecurityToken"))
            {
                throw new CredentialException(string.Format("Error retrieving credentials from IMDS result: {0}.",
                    jsonContent));
            }

            try
            {
                contentAccessKeyId = contentObj["AccessKeyId"];
                contentAccessKeySecret = contentObj["AccessKeySecret"];
                contentSecurityToken = contentObj["SecurityToken"];
                contentExpiration = contentObj["Expiration"];
            }
            catch
            {
                throw new CredentialException("Invalid json got from ECS Metadata service.");
            }

            var expirationStr = contentExpiration.Replace('T', ' ').Replace('Z', ' ');
            var dt = Convert.ToDateTime(expirationStr);
            var expiration = dt.GetTimeMillis();
            var credentialModel = new CredentialModel
            {
                AccessKeyId = contentAccessKeyId,
                AccessKeySecret = contentAccessKeySecret,
                SecurityToken = contentSecurityToken,
                Expiration = expiration,
                Type = AuthConstant.EcsRamRole,
                ProviderName = GetProviderName()
            };
            return new RefreshResult<CredentialModel>.Builder(credentialModel).StaleTime(GetStaleTime(expiration))
                .PrefetchTime(GetPrefetchTime(expiration)).Build();
        }

        private long GetPrefetchTime(long expiration)
        {
            var currentTimeMillis = DateTime.UtcNow.GetTimeMillis();
            return expiration <= 0 ? currentTimeMillis + 5 * 60 * 1000 : currentTimeMillis + 60 * 60 * 1000;
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

        public override string GetProviderName()
        {
            return "ecs_ram_role";
        }

        public int ConnectTimeout
        {
            get { return connectTimeout; }
        }

        public int ReadTimeout
        {
            get { return readTimeout; }
        }
    }
}