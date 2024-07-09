using System.Threading.Tasks;
using Aliyun.Credentials;
using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Utils;

using Xunit;

namespace aliyun_net_credentials_unit_tests
{
    public class CredentialTest
    {
        [Fact]
        public void CredentialAccessKeyTest()
        {
            Config config = new Config
            {
                AccessKeyId = "AccessKeyId",
                AccessKeySecret = "AccessKeySecret",
                Type = AuthConstant.AccessKey
            };
            Client credential = new Client(config);
            Assert.NotNull(credential);
            // Obsolete methods
            Assert.Equal("AccessKeyId", credential.GetAccessKeyId());
            Assert.Equal("AccessKeySecret", credential.GetAccessKeySecret());
            Assert.Equal(AuthConstant.AccessKey, credential.GetType());
            Assert.Null(credential.GetSecurityToken());
            Assert.Null(credential.GetBearerToken());

            // new methods
            Assert.Equal("AccessKeyId", credential.GetCredential().AccessKeyId);
            Assert.Equal("AccessKeySecret", credential.GetCredential().AccessKeySecret);
            Assert.Equal(AuthConstant.AccessKey, credential.GetCredential().Type);
            Assert.Null(credential.GetCredential().SecurityToken);
            Assert.Null(credential.GetCredential().BearerToken);

            Config configBearerToken = new Config
            {
                Type = AuthConstant.BeareaToken,
                BearerToken = "bearer"
            };
            Client credentialBearer = new Client(configBearerToken);
            Assert.NotNull(credentialBearer);
            Assert.Equal("bearer", credentialBearer.GetBearerToken());
            Assert.Equal("bearer", credentialBearer.GetCredential().BearerToken);
            Config creConfig = new Config()
            {
                Type = "ram_role_arn",
                AccessKeyId = "testkey",
                AccessKeySecret = "testSecret",
                RoleArn = "acs:ram::userId:role/testla",
                RoleSessionName = "justHello",
                Policy = "test",
                RoleSessionExpiration = 1800
            };
            Assert.Throws<CredentialException>(() =>  new Client(creConfig).GetCredential());
        }

        [Fact]
        public async Task CredentialAccessKeyAsyncTest()
        {
            Config config = new Config
            {
                AccessKeyId = "AccessKeyId",
                AccessKeySecret = "AccessKeySecret",
                Type = AuthConstant.AccessKey
            };
            Client credential = new Client(config);
            Assert.NotNull(credential);
            // Obsolete methods
            Assert.Equal("AccessKeyId", await credential.GetAccessKeyIdAsync());
            Assert.Equal("AccessKeySecret", await credential.GetAccessKeySecretAsync());
            Assert.Equal(AuthConstant.AccessKey, await credential.GetTypeAsync());
            Assert.Null(await credential.GetSecurityTokenAsync());
            Assert.Null(await credential.GetBearerTokenAsync());

            // new methods
            Assert.Equal("AccessKeyId", (await credential.GetCredentialAsync()).AccessKeyId);
            Assert.Equal("AccessKeySecret", (await credential.GetCredentialAsync()).AccessKeySecret);
            Assert.Equal(AuthConstant.AccessKey, (await credential.GetCredentialAsync()).Type);
            Assert.Null((await credential.GetCredentialAsync()).SecurityToken);
            Assert.Null((await credential.GetCredentialAsync()).BearerToken);

            Config configBearerToken = new Config
            {
                Type = AuthConstant.BeareaToken,
                BearerToken = "bearer"
            };
            Client credentialBearer = new Client(configBearerToken);
            Assert.NotNull(credentialBearer);
            Assert.Equal("bearer", await credentialBearer.GetBearerTokenAsync());
            Assert.Equal("bearer", (await credentialBearer.GetCredentialAsync()).BearerToken);
        }
    }
}
