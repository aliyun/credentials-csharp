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
        }

        [Fact]
        public void CredentialAllTest()
        {
            Config config = new Config();
            config.AccessKeyId = "AccessKeyId";
            config.AccessKeySecret = "AccessKeySecret";
            config.SecurityToken = "SecurityToken";
            config.RoleName = "test";
            config.Type = AuthConstant.Sts;
            Client credential = new Client(config);
            Assert.NotNull(credential);
            Assert.Equal("AccessKeyId", credential.GetAccessKeyId());
            Assert.Equal("AccessKeySecret", credential.GetAccessKeySecret());
            Assert.Equal("SecurityToken", credential.GetSecurityToken());

            config.Type = AuthConstant.EcsRamRole;
            Assert.IsType<EcsRamRoleCredentialProvider>(TestHelper.RunInstanceMethod(typeof(Client), "GetProvider", credential, new object[] { config }));

            config.Type = AuthConstant.RamRoleArn;
            Assert.IsType<RamRoleArnCredentialProvider>(TestHelper.RunInstanceMethod(typeof(Client), "GetProvider", credential, new object[] { config }));

            config.Type = AuthConstant.RsaKeyPair;
            Assert.IsType<RsaKeyPairCredentialProvider>(TestHelper.RunInstanceMethod(typeof(Client), "GetProvider", credential, new object[] { config }));

            config.Type = "default";
            string temp = AuthUtils.EnvironmentEcsMetaData;
            AuthUtils.EnvironmentEcsMetaData = string.Empty;
            Assert.Throws<CredentialException>(() => { credential = new Client(config); });

            AuthUtils.EnvironmentEcsMetaData = "EnvironmentEcsMetaData";
            Assert.IsType<DefaultCredentialsProvider>(TestHelper.RunInstanceMethod(typeof(Client), "GetProvider", credential, new object[] { config }));

            config.Type = null;
            Assert.IsType<DefaultCredentialsProvider>(TestHelper.RunInstanceMethod(typeof(Client), "GetProvider", credential, new object[] { config }));
            AuthUtils.EnvironmentEcsMetaData = temp;
        }
    }
}
