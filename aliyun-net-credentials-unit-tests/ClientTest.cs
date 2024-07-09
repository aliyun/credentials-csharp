using System;
using Aliyun.Credentials;
using Aliyun.Credentials.Utils;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Provider;
using Aliyun.Credentials.Exceptions;

using Xunit;

namespace aliyun_net_credentials_unit_tests
{
    public class ClientTest
    {
        [Fact]
        public void TestGetProvider()
        {
            Config config = new Config
            {
                Type = AuthConstant.AccessKey,
                RoleName = "test",
                AccessKeyId = null,
                AccessKeySecret = "AccessKeySecret"
            };

            var exception = Assert.Throws<ArgumentNullException>(() => new AccessKeyCredential(null, "test"));
            Assert.StartsWith("Access key ID cannot be null.", exception.Message);

            config.AccessKeyId = "AccessKeyId";
            Client client = new Client(config);
            config.Type = AuthConstant.EcsRamRole;
            var result = TestHelper.RunInstanceMethod(typeof(Client), "GetProvider", client, new object[] { config });
            Assert.IsType<EcsRamRoleCredentialProvider>(result);

            config.Type = AuthConstant.RamRoleArn;
            result = TestHelper.RunInstanceMethod(typeof(Client), "GetProvider", client, new object[] { config });
            Assert.IsType<RamRoleArnCredentialProvider>(result);

            config.Type = AuthConstant.RsaKeyPair;
            config.PublicKeyId = "test";
            config.PrivateKeyFile = "/test";
            result = TestHelper.RunInstanceMethod(typeof(Client), "GetProvider", client, new object[] { config });
            Assert.IsType<RsaKeyPairCredentialProvider>(result);

            config.Type = null;
            result = TestHelper.RunInstanceMethod(typeof(Client), "GetProvider", client, new object[] { config });
            Assert.IsType<DefaultCredentialsProvider>(result);

            config.Type = "default";
            result = TestHelper.RunInstanceMethod(typeof(Client), "GetProvider", client, new object[] { config });
            Assert.IsType<DefaultCredentialsProvider>(result);

            config.Type = AuthConstant.Sts;
            config.SecurityToken = "test";
            result = TestHelper.RunInstanceMethod(typeof(Client), "GetProvider", client, new object[] { config });
            Assert.IsType<StaticCredentialsProvider>(result);
        }

        [Fact]
        public void TestConstructor()
        {
            var ex = Assert.Throws<CredentialException>(() => new Client().GetCredential());
            Assert.StartsWith("Failed to connect ECS Metadata Service: ", ex.Message);

            ex = Assert.Throws<CredentialException>(() => new Client(null).GetCredential());
            Assert.StartsWith("Failed to connect ECS Metadata Service: ", ex.Message);
        }
    }
}