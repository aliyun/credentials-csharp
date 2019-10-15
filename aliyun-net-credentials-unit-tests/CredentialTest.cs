using Aliyun.Credentials;
using Aliyun.Credentials.Exceptions;
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
            Configuration config = new Configuration();
            config.AccessKeyId = "AccessKeyId";
            config.AccessKeySecret = "AccessKeySecret";
            config.Type = AuthConstant.AccessKey;
            Credential credential = new Credential(config);
            Assert.NotNull(credential);
            Assert.Equal("AccessKeyId", credential.AccessKeyId);
            Assert.Equal("AccessKeySecret", credential.AccessKeySecret);
            Assert.Equal(AuthConstant.AccessKey, credential.Type);
        }

        [Fact]
        public void CredentialAllTest()
        {
            Configuration config = new Configuration();
            config.AccessKeyId = "AccessKeyId";
            config.AccessKeySecret = "AccessKeySecret";
            config.SecurityToken = "SecurityToken";
            config.RoleName = "test";
            config.Type = AuthConstant.Sts;
            Credential credential = new Credential(config);
            Assert.NotNull(credential);
            Assert.Equal("AccessKeyId", credential.AccessKeyId);
            Assert.Equal("AccessKeySecret", credential.AccessKeySecret);
            Assert.Equal("SecurityToken", credential.SecurityToken);

            config.Type = AuthConstant.EcsRamRole;
            Assert.IsType<EcsRamRoleCredentialProvider>(TestHelper.RunInstanceMethod(typeof(Credential), "GetProvider", credential, new object[] { config }));

            config.Type = AuthConstant.RamRoleArn;
            Assert.IsType<RamRoleArnCredentialProvider>(TestHelper.RunInstanceMethod(typeof(Credential), "GetProvider", credential, new object[] { config }));

            config.Type = AuthConstant.RsaKeyPair;
            Assert.IsType<RsaKeyPairCredentialProvider>(TestHelper.RunInstanceMethod(typeof(Credential), "GetProvider", credential, new object[] { config }));

            config.Type = "default";
            string temp = AuthUtils.EnvironmentEcsMetaData;
            AuthUtils.EnvironmentEcsMetaData = string.Empty;
            Assert.Throws<CredentialException>(() => { credential = new Credential(config); });

            AuthUtils.EnvironmentEcsMetaData = "EnvironmentEcsMetaData";
            Assert.IsType<DefaultCredentialsProvider>(TestHelper.RunInstanceMethod(typeof(Credential), "GetProvider", credential, new object[] { config }));

            config.Type = null;
            Assert.IsType<DefaultCredentialsProvider>(TestHelper.RunInstanceMethod(typeof(Credential), "GetProvider", credential, new object[] { config }));
            AuthUtils.EnvironmentEcsMetaData = temp;
        }
    }
}
