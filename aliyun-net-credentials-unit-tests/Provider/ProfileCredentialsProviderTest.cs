using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Provider;
using Aliyun.Credentials.Utils;
using Aliyun.Credentials.Models;

using Xunit;
using Moq;

namespace aliyun_net_credentials_unit_tests.Provider
{
    public class ProfileCredentialsProviderTest
    {
        [Fact]
        public void ProfileGetCredentialTest()
        {
            ProfileCredentialsProvider provider = new ProfileCredentialsProvider();
            var ex = Assert.Throws<CredentialException>(() => provider.GetCredentials());
            Assert.StartsWith("Unable to open credentials file:", ex.Message);

            string tempEnvironmentCredentialsFile = AuthUtils.EnvironmentCredentialsFile;
            string tempClientType = AuthUtils.ClientType;
            AuthUtils.EnvironmentCredentialsFile = string.Empty;
            Assert.Throws<CredentialException>(() => { provider.GetCredentials(); });

            AuthUtils.EnvironmentCredentialsFile = TestHelper.GetIniFilePath();
            AuthUtils.ClientType = "default";
            Assert.NotNull(provider.GetCredentials());

            AuthUtils.ClientType = "client2";
            Assert.Throws<CredentialException>(() => { provider.GetCredentials(); });

            AuthUtils.ClientType = "client4";
            AuthUtils.SetPrivateKey("test");
            Assert.Throws<CredentialException>(() => { provider.GetCredentials(); });

            AuthUtils.ClientType = "client1";
            Assert.Throws<CredentialException>(() => { provider.GetCredentials(); });

            AuthUtils.ClientType = "client5";
            Assert.Equal("The configured client type is empty",
                Assert.Throws<CredentialException>(() => { provider.GetCredentials(); }).Message);

            AuthUtils.ClientType = "clientNotExit";
            Assert.Equal("Client is not open in the specified credentials file",
                Assert.Throws<CredentialException>(() => { provider.GetCredentials(); }).Message);

            AuthUtils.ClientType = "client6";
            Assert.Null(provider.GetCredentials());

            AuthUtils.ClientType = "client7";
            Assert.Null(provider.GetCredentials());

            AuthUtils.ClientType = "client8";
            Assert.Equal("OIDCTokenFilePath path does not exist.",
                Assert.Throws<CredentialException>(() => { provider.GetCredentials(); }).Message);

            AuthUtils.EnvironmentCredentialsFile = tempEnvironmentCredentialsFile;
            AuthUtils.ClientType = tempClientType;
        }

        [Fact]
        public async Task ProfileGetCredentialAsyncTest()
        {
            ProfileCredentialsProvider provider = new ProfileCredentialsProvider();
            Assert.Null(await provider.GetCredentialsAsync());

            string tempEnvironmentCredentialsFile = AuthUtils.EnvironmentCredentialsFile;
            string tempClientType = AuthUtils.ClientType;
            AuthUtils.EnvironmentCredentialsFile = string.Empty;
            await Assert.ThrowsAsync<CredentialException>(async () => { await provider.GetCredentialsAsync(); });

            AuthUtils.EnvironmentCredentialsFile = TestHelper.GetIniFilePath();
            AuthUtils.ClientType = "default";
            Assert.NotNull(provider.GetCredentialsAsync());

            AuthUtils.ClientType = "client2";
            await Assert.ThrowsAsync<CredentialException>(async () => { await provider.GetCredentialsAsync(); });

            AuthUtils.ClientType = "client4";
            AuthUtils.SetPrivateKey("test");
            await Assert.ThrowsAsync<CredentialException>(async () => { await provider.GetCredentialsAsync(); });

            AuthUtils.ClientType = "client1";
            await Assert.ThrowsAsync<CredentialException>(async () => { await provider.GetCredentialsAsync(); });

            AuthUtils.ClientType = "client5";
            Assert.Equal("The configured client type is empty",
                (await Assert.ThrowsAsync<CredentialException>(async () => { await provider.GetCredentialsAsync(); })).Message);

            AuthUtils.ClientType = "clientNotExit";
            Assert.Equal("Client is not open in the specified credentials file",
                (await Assert.ThrowsAsync<CredentialException>(async () => { await provider.GetCredentialsAsync(); })).Message);

            AuthUtils.ClientType = "client6";
            Assert.Null(await provider.GetCredentialsAsync());

            AuthUtils.ClientType = "client7";
            Assert.Null(await provider.GetCredentialsAsync());

            AuthUtils.ClientType = "client8";
            Assert.Equal("OIDCTokenFilePath path does not exist.",
                (await Assert.ThrowsAsync<CredentialException>(async() => { await provider.GetCredentialsAsync(); })).Message);

            AuthUtils.EnvironmentCredentialsFile = tempEnvironmentCredentialsFile;
            AuthUtils.ClientType = tempClientType;
        }

        [Fact]
        public void GetSTSAssumeRoleSessionCredentialsEmptyTest()
        {
            ProfileCredentialsProvider provider = new ProfileCredentialsProvider();
            Dictionary<string, string> clientConfig = new Dictionary<string, string>();
            Assert.Equal("The configured access_key_id or access_key_secret is empty",
                Assert.Throws<CredentialException>(() => { provider.GetSTSAssumeRoleSessionCredentials(clientConfig); }).Message);

            clientConfig.Add(AuthConstant.IniAccessKeyId, "IniAccessKeyId");
            Assert.Equal("The configured access_key_id or access_key_secret is empty",
                Assert.Throws<CredentialException>(() => { provider.GetSTSAssumeRoleSessionCredentials(clientConfig); }).Message);

            clientConfig.Add(AuthConstant.IniAccessKeyIdsecret, "IniAccessKeyIdsecret");
            Assert.Equal("The configured role_session_name or role_arn is empty",
                Assert.Throws<CredentialException>(() => { provider.GetSTSAssumeRoleSessionCredentials(clientConfig); }).Message);

            clientConfig.Add(AuthConstant.IniRoleSessionName, "IniRoleSessionName");
            Assert.Equal("The configured role_session_name or role_arn is empty",
                Assert.Throws<CredentialException>(() => { provider.GetSTSAssumeRoleSessionCredentials(clientConfig); }).Message);
        }

        [Fact]
        public async Task GetSTSAssumeRoleSessionCredentialsEmptyAsyncTest()
        {
            ProfileCredentialsProvider provider = new ProfileCredentialsProvider();
            Dictionary<string, string> clientConfig = new Dictionary<string, string>();
            Assert.Equal("The configured access_key_id or access_key_secret is empty",
                (await Assert.ThrowsAsync<CredentialException>(async () => { await provider.GetSTSAssumeRoleSessionCredentialsAsync(clientConfig); })).Message);

            clientConfig.Add(AuthConstant.IniAccessKeyId, "IniAccessKeyId");
            Assert.Equal("The configured access_key_id or access_key_secret is empty",
                (await Assert.ThrowsAsync<CredentialException>(async () => { await provider.GetSTSAssumeRoleSessionCredentialsAsync(clientConfig); })).Message);

            clientConfig.Add(AuthConstant.IniAccessKeyIdsecret, "IniAccessKeyIdsecret");
            Assert.Equal("The configured role_session_name or role_arn is empty",
                (await Assert.ThrowsAsync<CredentialException>(async () => { await provider.GetSTSAssumeRoleSessionCredentialsAsync(clientConfig); })).Message);

            clientConfig.Add(AuthConstant.IniRoleSessionName, "IniRoleSessionName");
            Assert.Equal("The configured role_session_name or role_arn is empty",
                (await Assert.ThrowsAsync<CredentialException>(async () => { await provider.GetSTSAssumeRoleSessionCredentialsAsync(clientConfig); })).Message);
        }

        [Fact]
        public void GetSTSGetSessionAccessKeyCredentialsEmptyTest()
        {
            ProfileCredentialsProvider provider = new ProfileCredentialsProvider();
            Dictionary<string, string> clientConfig = new Dictionary<string, string>();
            Assert.Equal("The configured private_key_file is empty",
                Assert.Throws<CredentialException>(() => { provider.GetSTSGetSessionAccessKeyCredentials(clientConfig); }).Message);

            clientConfig.Add(AuthConstant.IniPrivateKeyFile, "IniPrivateKeyFile");
            AuthUtils.SetPrivateKey("test");
            Assert.Equal("The configured public_key_id or private_key_file content is empty",
                Assert.Throws<CredentialException>(() => { provider.GetSTSGetSessionAccessKeyCredentials(clientConfig); }).Message);

        }

        [Fact]
        public async Task GetSTSGetSessionAccessKeyCredentialsEmptyAsyncTest()
        {
            ProfileCredentialsProvider provider = new ProfileCredentialsProvider();
            Dictionary<string, string> clientConfig = new Dictionary<string, string>();
            Assert.Equal("The configured private_key_file is empty",
                (await Assert.ThrowsAsync<CredentialException>(async () => { await provider.GetSTSGetSessionAccessKeyCredentialsAsync(clientConfig); })).Message);

            clientConfig.Add(AuthConstant.IniPrivateKeyFile, "IniPrivateKeyFile");
            AuthUtils.SetPrivateKey("test");
            Assert.Equal("The configured public_key_id or private_key_file content is empty",
                (await Assert.ThrowsAsync<CredentialException>(async () => { await provider.GetSTSGetSessionAccessKeyCredentialsAsync(clientConfig); })).Message);

        }

        [Fact]
        public void GetInstanceProfileCredentialsEmptyTest()
        {
            ProfileCredentialsProvider provider = new ProfileCredentialsProvider();
            Dictionary<string, string> clientConfig = new Dictionary<string, string>();
            Assert.Equal("The configured role_name is empty",
                Assert.Throws<CredentialException>(() => { provider.GetInstanceProfileCredentials(clientConfig); }).Message);
        }

        [Fact]
        public async Task GetInstanceProfileCredentialsEmptyAsyncTest()
        {
            ProfileCredentialsProvider provider = new ProfileCredentialsProvider();
            Dictionary<string, string> clientConfig = new Dictionary<string, string>();
            Assert.Equal("The configured role_name is empty",
                (await Assert.ThrowsAsync<CredentialException>(async () => { await provider.GetInstanceProfileCredentialsAsync(clientConfig); })).Message);
        }

        [Fact]
        public void GetSTSOIDCRoleSessionCredentialsTest()
        {
            ProfileCredentialsProvider provider = new ProfileCredentialsProvider();
            Dictionary<string, string> clientConfig = new Dictionary<string, string>();
            Assert.Equal("The configured role_arn is empty",
                Assert.Throws<CredentialException>(() => { provider.GetSTSOIDCRoleSessionCredentials(clientConfig); }).Message);

            clientConfig.Add(AuthConstant.IniRoleArn, "IniRoleArn");
            Assert.Equal("The configured oidc_provider_arn is empty",
                Assert.Throws<CredentialException>(() => { provider.GetSTSOIDCRoleSessionCredentials(clientConfig); }).Message);
        }

        [Fact]
        public async Task GetSTSOIDCRoleSessionCredentialsAsyncTest()
        {
            ProfileCredentialsProvider provider = new ProfileCredentialsProvider();
            Dictionary<string, string> clientConfig = new Dictionary<string, string>();
            Assert.Equal("The configured role_arn is empty",
                (await Assert.ThrowsAsync<CredentialException>(async () => { await provider.GetSTSOIDCRoleSessionCredentialsAsync(clientConfig); })).Message);

            clientConfig.Add(AuthConstant.IniRoleArn, "IniRoleArn");
            Assert.Equal("The configured oidc_provider_arn is empty",
                (await Assert.ThrowsAsync<CredentialException>(async () => { await provider.GetSTSOIDCRoleSessionCredentialsAsync(clientConfig); })).Message);
        }
    }
}
