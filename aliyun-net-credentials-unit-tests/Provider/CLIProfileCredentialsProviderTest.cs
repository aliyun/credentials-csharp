using System;
using System.IO;
using System.Reflection;

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
        public void GetProfileNameTest()
        {
            CLIProfileCredentialsProvider provider = new CLIProfileCredentialsProvider();
            Assert.Null(provider.GetProfileName());
            provider = new CLIProfileCredentialsProvider("AK");
            Assert.Equal("AK", provider.GetProfileName());
            Assert.Equal("cli_profile", provider.GetProviderName());

            Environment.SetEnvironmentVariable("ALIBABA_CLOUD_PROFILE", "TEST");
            provider = new CLIProfileCredentialsProvider();
            Assert.Equal("TEST", provider.GetProfileName());
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
            Assert.Equal("[{\"name\":\"AK\",\"mode\":\"AK\",\"access_key_id\":\"access_key_id\",\"access_key_secret\":\"access_key_secret\"},{\"name\":\"RamRoleArn\",\"mode\":\"RamRoleArn\",\"access_key_id\":\"access_key_id\",\"access_key_secret\":\"access_key_secret\",\"ram_role_arn\":\"ram_role_arn\",\"ram_session_name\":\"ram_session_name\",\"expired_seconds\":3600,\"sts_region\":\"cn-hangzhou\"},{\"name\":\"EcsRamRole\",\"mode\":\"EcsRamRole\",\"ram_role_name\":\"ram_role_name\"},{\"name\":\"OIDC\",\"mode\":\"OIDC\",\"ram_role_arn\":\"ram_role_arn\",\"ram_session_name\":\"ram_session_name\",\"expired_seconds\":3600,\"sts_region\":\"cn-hangzhou\",\"oidc_token_file\":\"path/to/oidc/file\",\"oidc_provider_arn\":\"oidc_provider_arn\"},{\"name\":\"ChainableRamRoleArn\",\"mode\":\"ChainableRamRoleArn\",\"ram_role_arn\":\"ram_role_arn\",\"ram_session_name\":\"ram_session_name\",\"expired_seconds\":3600,\"sts_region\":\"cn-hangzhou\",\"source_profile\":\"AK\"}]", JsonConvert.SerializeObject(config.GetProfiles(), settings));
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

            credentialsProvider = provider.ReloadCredentialsProvider(config, "RamRoleArn");
            Assert.True(credentialsProvider is RamRoleArnCredentialProvider);
            ex = Assert.Throws<CredentialException>(() => { credentialsProvider.GetCredentials(); });
            Assert.Contains("InvalidAccessKeyId.NotFound", ex.Message);

            var exAsync = await Assert.ThrowsAsync<CredentialException>(async() => {await credentialsProvider.GetCredentialsAsync(); });
            Assert.Contains("InvalidAccessKeyId.NotFound", ex.Message);

            var ex1 = Assert.Throws<ArgumentNullException>(() => { provider.ReloadCredentialsProvider(config, "Invalid_RamRoleArn"); });
            Assert.Contains("AccessKeyId must not be null or empty.", ex1.Message);

            credentialsProvider = provider.ReloadCredentialsProvider(config, "EcsRamRole");
            Assert.True(credentialsProvider is EcsRamRoleCredentialProvider);

            credentialsProvider  = provider.ReloadCredentialsProvider(config, "OIDC");
            Assert.True(credentialsProvider is OIDCRoleArnCredentialProvider);

            credentialsProvider = provider.ReloadCredentialsProvider(config, "ChainableRamRoleArn");
            Assert.True(credentialsProvider is RamRoleArnCredentialProvider);
            Assert.Equal("akid", ((RamRoleArnCredentialProvider) credentialsProvider).CredentialsProvider.GetCredentials().AccessKeyId);
            Assert.Equal("secret", ((RamRoleArnCredentialProvider) credentialsProvider).CredentialsProvider.GetCredentials().AccessKeySecret);

            ex = Assert.Throws<CredentialException>(() => { provider.ReloadCredentialsProvider(config, "ChainableRamRoleArn2"); });
            Assert.Contains("Unable to get profile with 'InvalidSource' form CLI credentials file.", ex.Message);

            ex = Assert.Throws<CredentialException>(() => { provider.ReloadCredentialsProvider(config, "Unsupported"); });
            Assert.Contains("Unsupported profile mode 'Unsupported' form CLI credentials file.", ex.Message);
        }
    }
}
