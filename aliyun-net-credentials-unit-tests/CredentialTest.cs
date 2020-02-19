using System.Threading.Tasks;

using Aliyun.Credentials;
using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Provider;
using Aliyun.Credentials.Utils;

using Xunit;

namespace aliyun_net_credentials_unit_tests
{
    public class CredentialTest
    {
        [Fact]
        public void CredentialAccessKeyTest()
        {
            Config config = new Config();
            config.AccessKeyId = "AccessKeyId";
            config.AccessKeySecret = "AccessKeySecret";
            config.Type = AuthConstant.AccessKey;
            Client credential = new Client(config);
            Assert.NotNull(credential);
            Assert.Equal("AccessKeyId", credential.GetAccessKeyId());
            Assert.Equal("AccessKeySecret", credential.GetAccessKeySecret());
            Assert.Equal(AuthConstant.AccessKey, credential.GetType());
            Assert.Null(credential.GetSecurityToken());
            Assert.Null(credential.GetBearerToken());

            Config configBearerToken = new Config();
            configBearerToken.Type = AuthConstant.BeareaToken;
            configBearerToken.BearerToken = "bearer";
            Client credentialBearer = new Client(configBearerToken);
            Assert.NotNull(credentialBearer);
            Assert.Equal("bearer", credentialBearer.GetBearerToken());
        }

        [Fact]
        public async Task CredentialAccessKeyAsyncTest()
        {
            Config config = new Config();
            config.AccessKeyId = "AccessKeyId";
            config.AccessKeySecret = "AccessKeySecret";
            config.Type = AuthConstant.AccessKey;
            Client credential = new Client(config);
            Assert.NotNull(credential);
            Assert.Equal("AccessKeyId", await credential.GetAccessKeyIdAsync());
            Assert.Equal("AccessKeySecret", await credential.GetAccessKeySecretAsync());
            Assert.Equal(AuthConstant.AccessKey, await credential.GetTypeAsync());
            Assert.Null(await credential.GetSecurityTokenAsync());
            Assert.Null(await credential.GetBearerTokenAsync());

            Config configBearerToken = new Config();
            configBearerToken.Type = AuthConstant.BeareaToken;
            configBearerToken.BearerToken = "bearer";
            Client credentialBearer = new Client(configBearerToken);
            Assert.NotNull(credentialBearer);
            Assert.Equal("bearer", await credentialBearer.GetBearerTokenAsync());
        }
    }
}
