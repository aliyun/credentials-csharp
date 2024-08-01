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
            Assert.Null(provider.GetCredentials());

            //AuthUtils.ClientType = "default";
            AuthUtils.EnvironmentAccessKeyId = null;
            Assert.Null(provider.GetCredentials());

            AuthUtils.EnvironmentAccesskeySecret = null;
            Assert.Null(provider.GetCredentials());

            AuthUtils.EnvironmentAccessKeyId = string.Empty;
            AuthUtils.EnvironmentAccesskeySecret = string.Empty;
            Assert.Throws<CredentialException>(() => { provider.GetCredentials(); });

            AuthUtils.EnvironmentAccessKeyId = "AccessKeyId";
            Assert.Throws<CredentialException>(() => { provider.GetCredentials(); });

            AuthUtils.EnvironmentAccesskeySecret = "AccesskeySecret";
            Assert.IsType<CredentialModel>(provider.GetCredentials());
            CredentialModel credential = provider.GetCredentials();
            Assert.Equal("access_key", credential.Type);

            AuthUtils.EnvironmentSecurityToken = "SecurityToken";
            credential = provider.GetCredentials();
            Assert.Equal("sts", credential.Type);


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
            Assert.Null(await provider.GetCredentialsAsync());

            AuthUtils.ClientType = "default";
            AuthUtils.EnvironmentAccessKeyId = null;
            Assert.Null(await provider.GetCredentialsAsync());

            AuthUtils.EnvironmentAccesskeySecret = null;
            Assert.Null(await provider.GetCredentialsAsync());

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
