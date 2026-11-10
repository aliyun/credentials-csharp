using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Aliyun.Credentials.Http;
using Moq;

using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Provider;
using Aliyun.Credentials.Utils;
using Newtonsoft.Json;
using Xunit;

namespace aliyun_net_credentials_unit_tests.Provider
{
    public class CLIProfileCredentialsProviderTest
    {
        [Fact]
        public async Task SharedCloudSSOValidatesAndRefreshesTheSession()
        {
            string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(directory);
            try
            {
                string path = Path.Combine(directory, "config.json");
                File.WriteAllText(path, "{\"current\":\"dev\",\"profiles\":[{\"name\":\"dev\",\"mode\":\"CloudSSO\","
                    + "\"sso_session\":\"company\",\"cloud_sso_account_id\":\"account\",\"cloud_sso_access_config\":\"role\","
                    + "\"cloud_sso_sign_in_url\":\"https://example.test\",\"access_token\":\"inline\",\"cloud_sso_access_token_expire\":4102444800}]}");
                var cli = new CLIProfileCredentialsProvider("dev");
                var provider = cli.ReloadCredentialsProvider(cli.ParseProfile(path), "dev");
                var ex = Assert.Throws<CredentialException>(() => provider.GetCredentials());
                Assert.Contains("Unable to read CloudSSO session company", ex.Message);
                await Assert.ThrowsAsync<CredentialException>(() => provider.GetCredentialsAsync());

                string canonicalPath = (string)typeof(CLIProfileCredentialsProvider)
                    .GetMethod("CanonicalConfigPath", BindingFlags.Static | BindingFlags.NonPublic)
                    .Invoke(null, new object[] { path });
                string hash;
                using (var sha256 = SHA256.Create())
                {
                    hash = BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes(canonicalPath)))
                        .Replace("-", "").ToLowerInvariant();
                }
                string sessionDirectory = Path.Combine(directory, "sso", hash);
                Directory.CreateDirectory(sessionDirectory);
                string session = Path.Combine(sessionDirectory, "company.json");
                foreach (var entry in new[] {
                    new { Contents = "{", Message = "Invalid CloudSSO session" },
                    new { Contents = "{\"name\":\"wrong\"}", Message = "Invalid CloudSSO session" },
                    new { Contents = "{\"name\":\"company\",\"sign_in_url\":\"https://example.test\",\"access_token\":\"expired\",\"expires_at\":1}",
                          Message = "expired" }
                })
                {
                    File.WriteAllText(session, entry.Contents);
                    ex = Assert.Throws<CredentialException>(() => provider.GetCredentials());
                    Assert.Contains(entry.Message, ex.Message);
                }

                File.Delete(session);
                Directory.CreateDirectory(session);
                Assert.Contains("Unable to read CloudSSO session",
                    Assert.Throws<CredentialException>(() => provider.GetCredentials()).Message);
                Directory.Delete(session);

                var transport = new Mock<IConnClient>();
                var response = new HttpResponse("https://example.test/cloud-credentials")
                {
                    Status = 200, Encoding = "UTF-8",
                    Content = Encoding.UTF8.GetBytes("{\"CloudCredential\":{\"AccessKeyId\":\"ak\",\"AccessKeySecret\":\"sk\","
                        + "\"SecurityToken\":\"sts\",\"Expiration\":\"2099-01-01T00:00:00Z\"}}")
                };
                transport.Setup(client => client.DoAction(It.Is<HttpRequest>(
                    request => request.Headers["Authorization"] == "Bearer first"))).Returns(response);
                transport.Setup(client => client.DoActionAsync(It.Is<HttpRequest>(
                    request => request.Headers["Authorization"] == "Bearer second"))).ReturnsAsync(response);
                const string template = "{{\"name\":\"company\",\"sign_in_url\":\"https://example.test\",\"access_token\":\"{0}\",\"expires_at\":4102444800}}";
                File.WriteAllText(session, string.Format(template, "first"));
                var first = (RefreshResult<CredentialModel>)TestHelper.RunInstanceMethod(
                    typeof(CloudSSOCredentialProvider), "GetNewSessionCredentials", provider, new object[] { transport.Object });
                Assert.Equal("ak", first.Value.AccessKeyId);
                File.WriteAllText(session, string.Format(template, "second"));
                var second = (RefreshResult<CredentialModel>)TestHelper.RunInstanceMethodAsync(
                    typeof(CloudSSOCredentialProvider), "GetNewSessionCredentialsAsync", provider, new object[] { transport.Object });
                Assert.Equal("sts", second.Value.SecurityToken);

                File.WriteAllText(path, File.ReadAllText(path).Replace("company", "../outside"));
                var invalid = cli.ReloadCredentialsProvider(cli.ParseProfile(path), "dev");
                Assert.Contains("Invalid CloudSSO session name",
                    Assert.Throws<CredentialException>(() => invalid.GetCredentials()).Message);
                File.Delete(path);
                File.Delete(session);
                Assert.Contains("Unable to read CloudSSO session",
                    Assert.Throws<CredentialException>(() => provider.GetCredentials()).Message);


            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [Fact]
        public void GetProfileNameTest()
        {
            CLIProfileCredentialsProvider provider = new CLIProfileCredentialsProvider();
            Assert.Null(provider.GetProfileName());
            provider = new CLIProfileCredentialsProvider("AK");
            Assert.Equal("AK", provider.GetProfileName());
            Assert.Equal("cli_profile", provider.GetProviderName());

            var cacheProfile = Environment.GetEnvironmentVariable("ALIBABA_CLOUD_PROFILE");
            Environment.SetEnvironmentVariable("ALIBABA_CLOUD_PROFILE", "TEST");
            provider = new CLIProfileCredentialsProvider();
            Assert.Equal("TEST", provider.GetProfileName());
            Environment.SetEnvironmentVariable("ALIBABA_CLOUD_PROFILE", cacheProfile);

            string path = TestHelper.GetCLIConfigFilePath("aliyun");
            provider = new CLIProfileCredentialsProvider();
            var credential = provider.GetCredentials(path);
            Assert.Equal("cli_profile/static_ak", credential.ProviderName);

            Environment.SetEnvironmentVariable("ALIBABA_CLOUD_PROFILE", "AK");
            credential = provider.GetCredentials(path);
            Environment.SetEnvironmentVariable("ALIBABA_CLOUD_PROFILE", cacheProfile);

            path = TestHelper.GetCLIConfigFilePath("empty");
            var ex = Assert.Throws<CredentialException>(() => provider.GetCredentials(path));
            Assert.Equal("Unable to get profile form empty CLI credentials file.", ex.Message);
        }

        [Fact]
        public void ShouldReloadCredentialsProviderTest()
        {
            CLIProfileCredentialsProvider provider = new CLIProfileCredentialsProvider();
            Assert.True(provider.ShouldReloadCredentialsProvider(""));
        }

        [Fact]
        public void DisableCLIProfileTest()
        {
            bool isDisableCLIProfile = AuthUtils.EnvironmentDisableCLIProfile;
            AuthUtils.EnvironmentDisableCLIProfile = true;
            CLIProfileCredentialsProvider provider = new CLIProfileCredentialsProvider();
            var ex = Assert.Throws<CredentialException>(() => { provider.GetCredentials(); });
            Assert.Contains("CLI credentials file is disabled.", ex.Message);
            AuthUtils.EnvironmentDisableCLIProfile = isDisableCLIProfile;
        }

        [Fact]
        public void ParseProfileTest()
        {
            CLIProfileCredentialsProvider provider = new CLIProfileCredentialsProvider();
            var ex = Assert.Throws<CredentialException>(() => { provider.ParseProfile("./not_exist_config.json"); });
            Assert.Contains("Unable to open credentials file", ex.Message);

            string configPath = TestHelper.GetCLIConfigFilePath("invalid");
            ex = Assert.Throws<CredentialException>(() => { provider.ParseProfile(configPath); });
            Assert.Contains("Failed to parse credential from CLI credentials file", ex.Message);

            configPath = TestHelper.GetCLIConfigFilePath("empty");
            CLIProfileCredentialsProvider.Config config = provider.ParseProfile(configPath);
            Assert.Null(config);

            configPath = TestHelper.GetCLIConfigFilePath("mock_empty");
            config = provider.ParseProfile(configPath);
            Assert.NotNull(config);
            Assert.Null(config.GetCurrent());
            Assert.Null(config.GetProfiles());

            configPath = TestHelper.GetCLIConfigFilePath("full");
            config = provider.ParseProfile(configPath);
            Assert.Equal("AK", config.GetCurrent());
            Assert.Equal(5, config.GetProfiles().Count);
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            Assert.Equal("[{\"name\":\"AK\",\"mode\":\"AK\",\"access_key_id\":\"access_key_id\",\"access_key_secret\":\"access_key_secret\"},{\"name\":\"RamRoleArn\",\"mode\":\"RamRoleArn\",\"access_key_id\":\"access_key_id\",\"access_key_secret\":\"access_key_secret\",\"ram_role_arn\":\"ram_role_arn\",\"ram_session_name\":\"ram_session_name\",\"expired_seconds\":3600,\"sts_region\":\"cn-hangzhou\",\"enable_vpc\":true},{\"name\":\"EcsRamRole\",\"mode\":\"EcsRamRole\",\"ram_role_name\":\"ram_role_name\"},{\"name\":\"OIDC\",\"mode\":\"OIDC\",\"ram_role_arn\":\"ram_role_arn\",\"ram_session_name\":\"ram_session_name\",\"expired_seconds\":3600,\"sts_region\":\"cn-hangzhou\",\"oidc_token_file\":\"path/to/oidc/file\",\"oidc_provider_arn\":\"oidc_provider_arn\"},{\"name\":\"ChainableRamRoleArn\",\"mode\":\"ChainableRamRoleArn\",\"ram_role_arn\":\"ram_role_arn\",\"ram_session_name\":\"ram_session_name\",\"expired_seconds\":3600,\"sts_region\":\"cn-hangzhou\",\"source_profile\":\"AK\"}]", JsonConvert.SerializeObject(config.GetProfiles(), settings));
        }

        [Fact]
        public async void ReloadCredentialsProviderTest()
        {
            CLIProfileCredentialsProvider provider = new CLIProfileCredentialsProvider();
            string configPath = TestHelper.GetCLIConfigFilePath("aliyun");
            CLIProfileCredentialsProvider.Config config = provider.ParseProfile(configPath);
            var ex = Assert.Throws<CredentialException>(() => { provider.ReloadCredentialsProvider(config, "notExist"); });
            Assert.Contains("Unable to get profile with 'notExist' form CLI credentials file.", ex.Message);

            IAlibabaCloudCredentialsProvider credentialsProvider = provider.ReloadCredentialsProvider(config, "AK");
            Assert.True(credentialsProvider is StaticAKCredentialsProvider);
            CredentialModel credential = credentialsProvider.GetCredentials();
            Assert.Equal("akid", credential.AccessKeyId);
            Assert.Equal("secret", credential.AccessKeySecret);
            Assert.Equal("static_ak", credential.ProviderName);
            Assert.Null(credential.SecurityToken);

            credential = await credentialsProvider.GetCredentialsAsync();
            Assert.Equal("akid", credential.AccessKeyId);
            Assert.Equal("secret", credential.AccessKeySecret);
            Assert.Equal("static_ak", credential.ProviderName);
            Assert.Null(credential.SecurityToken);

            credentialsProvider = provider.ReloadCredentialsProvider(config, "StsToken");
            Assert.True(credentialsProvider is StaticSTSCredentialsProvider);
            credential = credentialsProvider.GetCredentials();
            Assert.Equal("accessKeyId", credential.AccessKeyId);
            Assert.Equal("accessKeySecret", credential.AccessKeySecret);
            Assert.Equal("stsToken", credential.SecurityToken);
            Assert.Equal("static_sts", credential.ProviderName);

            credential = await credentialsProvider.GetCredentialsAsync();
            Assert.Equal("accessKeyId", credential.AccessKeyId);
            Assert.Equal("accessKeySecret", credential.AccessKeySecret);
            Assert.Equal("static_sts", credential.ProviderName);
            Assert.Equal("stsToken", credential.SecurityToken);

            credentialsProvider = provider.ReloadCredentialsProvider(config, "RamRoleArn");
            Assert.True(credentialsProvider is RamRoleArnCredentialProvider);
            ex = Assert.Throws<CredentialException>(() => { credentialsProvider.GetCredentials(); });
            Assert.Contains("InvalidAccessKeyId.NotFound", ex.Message);

            var exAsync = await Assert.ThrowsAsync<CredentialException>(async() => {await credentialsProvider.GetCredentialsAsync(); });
            Assert.Contains("InvalidAccessKeyId.NotFound", ex.Message);

            credentialsProvider = provider.ReloadCredentialsProvider(config, "RamRoleArnEnableVpc");
            Assert.True(credentialsProvider is RamRoleArnCredentialProvider);
            ex = Assert.Throws<CredentialException>(() => { credentialsProvider.GetCredentials(); });
            Assert.Contains("The request url is sts-vpc.cn-hangzhou.aliyuncs.com", ex.Message);

            exAsync = await Assert.ThrowsAsync<CredentialException>(async() => {await credentialsProvider.GetCredentialsAsync(); });
            Assert.Contains("The request url is sts-vpc.cn-hangzhou.aliyuncs.com", ex.Message);

            var ex1 = Assert.Throws<ArgumentNullException>(() => { provider.ReloadCredentialsProvider(config, "Invalid_RamRoleArn"); });
            Assert.Contains("AccessKeyId must not be null or empty.", ex1.Message);

            credentialsProvider = provider.ReloadCredentialsProvider(config, "EcsRamRole");
            Assert.True(credentialsProvider is EcsRamRoleCredentialProvider);

            credentialsProvider  = provider.ReloadCredentialsProvider(config, "OIDC");
            Assert.True(credentialsProvider is OIDCRoleArnCredentialProvider);

            credentialsProvider  = provider.ReloadCredentialsProvider(config, "OIDCEnableVpc");
            Assert.True(credentialsProvider is OIDCRoleArnCredentialProvider);

            credentialsProvider = provider.ReloadCredentialsProvider(config, "ChainableRamRoleArn");
            Assert.True(credentialsProvider is RamRoleArnCredentialProvider);
            Assert.Equal("akid", ((RamRoleArnCredentialProvider) credentialsProvider).CredentialsProvider.GetCredentials().AccessKeyId);
            Assert.Equal("secret", ((RamRoleArnCredentialProvider) credentialsProvider).CredentialsProvider.GetCredentials().AccessKeySecret);

            ex = Assert.Throws<CredentialException>(() => { provider.ReloadCredentialsProvider(config, "ChainableRamRoleArn1"); });
            Assert.Equal("Source profile name can not be the same as profile name.", ex.Message);

            ex = Assert.Throws<CredentialException>(() => { provider.ReloadCredentialsProvider(config, "ChainableRamRoleArn2"); });
            Assert.Contains("Unable to get profile with 'InvalidSource' form CLI credentials file.", ex.Message);

            credentialsProvider = provider.ReloadCredentialsProvider(config, "CloudSSO");
            Assert.True(credentialsProvider is CloudSSOCredentialProvider);
            Assert.Equal("cloud_sso", ((CloudSSOCredentialProvider)credentialsProvider).GetProviderName());

            credentialsProvider = provider.ReloadCredentialsProvider(config, "OAuth");
            Assert.True(credentialsProvider is OAuthCredentialProvider);
            Assert.Equal("oauth", ((OAuthCredentialProvider)credentialsProvider).GetProviderName());

            ex = Assert.Throws<CredentialException>(() => { provider.ReloadCredentialsProvider(config, "OAuth_InvalidSite"); });
            Assert.Contains("Invalid OAuth site type, support CN or INTL.", ex.Message);

            credentialsProvider = provider.ReloadCredentialsProvider(config, "External");
            Assert.True(credentialsProvider is ExternalCredentialProvider);
            Assert.Equal("external", ((ExternalCredentialProvider)credentialsProvider).GetProviderName());
            if (Path.DirectorySeparatorChar != '\\')
            {
                // The fixture process_command uses /usr/bin/printf, unavailable on Windows.
                credential = credentialsProvider.GetCredentials();
                Assert.Equal("externalAk", credential.AccessKeyId);
                Assert.Equal("externalSk", credential.AccessKeySecret);
            }

            ex = Assert.Throws<CredentialException>(() => { provider.ReloadCredentialsProvider(config, "Unsupported"); });
            Assert.Contains("Unsupported profile mode 'Unsupported' form CLI credentials file.", ex.Message);
        }
    }
}
