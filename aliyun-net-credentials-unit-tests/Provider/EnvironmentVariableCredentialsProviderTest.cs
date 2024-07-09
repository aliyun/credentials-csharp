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

            AuthUtils.ClientType = tempClientType;
            AuthUtils.EnvironmentAccessKeyId = tempEnvironmentAccessKeyId;
            AuthUtils.EnvironmentAccesskeySecret = tempEnvironmentAccesskeySecret;
        }

        [Fact]
        public async Task EnvironmentVariableProviderAsyncTest()
        {
            EnvironmentVariableCredentialsProvider provider = new EnvironmentVariableCredentialsProvider();
            string tempClientType = AuthUtils.ClientType;
            string tempEnvironmentAccessKeyId = AuthUtils.EnvironmentAccessKeyId;
            string tempEnvironmentAccesskeySecret = AuthUtils.EnvironmentAccesskeySecret;
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

            AuthUtils.ClientType = tempClientType;
            AuthUtils.EnvironmentAccessKeyId = tempEnvironmentAccessKeyId;
            AuthUtils.EnvironmentAccesskeySecret = tempEnvironmentAccesskeySecret;
        }
    }
}
