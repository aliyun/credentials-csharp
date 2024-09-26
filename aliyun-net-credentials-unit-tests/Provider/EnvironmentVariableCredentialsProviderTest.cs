using System;
using System.Threading.Tasks;

using Aliyun.Credentials;
using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Provider;
using Aliyun.Credentials.Utils;

using Xunit;

namespace aliyun_net_credentials_unit_tests.Provider
{
    public class EnvironmentVariableCredentialsProviderTest
    {
        [Fact]
        public void EnvironmentVariableProviderTest()
        {
            EnvironmentVariableCredentialsProvider provider = new EnvironmentVariableCredentialsProvider();
            string tempClientType = AuthUtils.ClientType;
            string tempEnvironmentAccessKeyId = AuthUtils.EnvironmentAccessKeyId;
            string tempEnvironmentAccesskeySecret = AuthUtils.EnvironmentAccesskeySecret;
            string tempEnvironmentSecurityToken = AuthUtils.EnvironmentSecurityToken;
            AuthUtils.ClientType = string.Empty;
            var ex = Assert.Throws<CredentialException>(() => { provider.GetCredentials(); });
            Assert.Equal("Environment variable accessKeyId cannot be empty", ex.Message);

            AuthUtils.EnvironmentAccessKeyId = null;
            ex = Assert.Throws<CredentialException>(() => { provider.GetCredentials(); });
            Assert.Equal("Environment variable accessKeyId cannot be empty", ex.Message);

            AuthUtils.EnvironmentAccesskeySecret = null;
            ex = Assert.Throws<CredentialException>(() => { provider.GetCredentials(); });
            Assert.Equal("Environment variable accessKeyId cannot be empty", ex.Message);

            AuthUtils.EnvironmentAccessKeyId = string.Empty;
            AuthUtils.EnvironmentAccesskeySecret = string.Empty;
            Assert.Throws<CredentialException>(() => { provider.GetCredentials(); });

            AuthUtils.EnvironmentAccessKeyId = "AccessKeyId";
            Assert.Throws<CredentialException>(() => { provider.GetCredentials(); });

            AuthUtils.EnvironmentAccesskeySecret = "AccesskeySecret";
            Assert.IsType<CredentialModel>(provider.GetCredentials());
            CredentialModel credential = provider.GetCredentials();
            Assert.Equal("access_key", credential.Type);
            Assert.Equal("env", credential.ProviderName);

            AuthUtils.EnvironmentSecurityToken = "SecurityToken";
            credential = provider.GetCredentials();
            Assert.Equal("sts", credential.Type);
            Assert.Equal("env", credential.ProviderName);


            AuthUtils.ClientType = tempClientType;
            AuthUtils.EnvironmentAccessKeyId = tempEnvironmentAccessKeyId;
            AuthUtils.EnvironmentAccesskeySecret = tempEnvironmentAccesskeySecret;
            AuthUtils.EnvironmentSecurityToken = tempEnvironmentSecurityToken;
        }

        [Fact]
        public async Task EnvironmentVariableProviderAsyncTest()
        {
            EnvironmentVariableCredentialsProvider provider = new EnvironmentVariableCredentialsProvider();
            string tempClientType = AuthUtils.ClientType;
            string tempEnvironmentAccessKeyId = AuthUtils.EnvironmentAccessKeyId;
            string tempEnvironmentAccesskeySecret = AuthUtils.EnvironmentAccesskeySecret;
            string tempEnvironmentSecurityToken = AuthUtils.EnvironmentSecurityToken;
            AuthUtils.ClientType = string.Empty;
            var ex = await Assert.ThrowsAsync<CredentialException>(async () => { await provider.GetCredentialsAsync(); });
            Assert.Equal("Environment variable accessKeyId cannot be empty", ex.Message);

            AuthUtils.ClientType = "default";
            AuthUtils.EnvironmentAccessKeyId = null;
            ex = await Assert.ThrowsAsync<CredentialException>(async () => { await provider.GetCredentialsAsync(); });
            Assert.Equal("Environment variable accessKeyId cannot be empty", ex.Message);

            AuthUtils.EnvironmentAccesskeySecret = null;
            ex = await Assert.ThrowsAsync<CredentialException>(async () => { await provider.GetCredentialsAsync(); });
            Assert.Equal("Environment variable accessKeyId cannot be empty", ex.Message);

            AuthUtils.EnvironmentAccessKeyId = string.Empty;
            AuthUtils.EnvironmentAccesskeySecret = string.Empty;
            await Assert.ThrowsAsync<CredentialException>(async() => { await provider.GetCredentialsAsync(); });

            AuthUtils.EnvironmentAccessKeyId = "AccessKeyId";
            await Assert.ThrowsAsync<CredentialException>(async() => { await provider.GetCredentialsAsync(); });

            AuthUtils.EnvironmentAccesskeySecret = "AccesskeySecret";
            Assert.IsType<CredentialModel>(await provider.GetCredentialsAsync());
            CredentialModel credential = await provider.GetCredentialsAsync();
            Assert.Equal("access_key", credential.Type);

            AuthUtils.EnvironmentSecurityToken = "SecurityToken";
            credential = await provider.GetCredentialsAsync();
            Assert.Equal("sts", credential.Type);

            AuthUtils.ClientType = tempClientType;
            AuthUtils.EnvironmentAccessKeyId = tempEnvironmentAccessKeyId;
            AuthUtils.EnvironmentAccesskeySecret = tempEnvironmentAccesskeySecret;
            AuthUtils.EnvironmentSecurityToken = tempEnvironmentSecurityToken;
        }
    }
}
