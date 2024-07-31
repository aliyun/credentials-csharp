using Aliyun.Credentials.Utils;

using Xunit;

namespace aliyun_net_credentials_unit_tests.Utils
{
    public class AuthUtilsTest
    {
        [Fact]
        public void GetPrivateKeyTest()
        {
            string privatKey = AuthUtils.GetPrivateKey(TestHelper.GetIniFilePath());

            Assert.NotNull(privatKey);
            Assert.NotEmpty(privatKey);
        }

        [Fact]
        public void EnvironmentTest()
        {
            AuthUtils.ClientType = null;
            Assert.Equal("default", AuthUtils.ClientType);
            AuthUtils.ClientType = "test";
            Assert.Equal("test", AuthUtils.ClientType);

            Assert.Null(AuthUtils.EnvironmentAccessKeyId);
            AuthUtils.EnvironmentAccessKeyId = "test";
            Assert.Equal("test", AuthUtils.EnvironmentAccessKeyId);
            AuthUtils.EnvironmentAccessKeyId = null;

            Assert.Null(AuthUtils.EnvironmentAccesskeySecret);
            AuthUtils.EnvironmentAccesskeySecret = "test";
            Assert.Equal("test", AuthUtils.EnvironmentAccesskeySecret);
            AuthUtils.EnvironmentAccesskeySecret = null;

            AuthUtils.EnvironmentCredentialsFile = "test";
            Assert.Equal("test", AuthUtils.EnvironmentCredentialsFile);
            AuthUtils.EnvironmentCredentialsFile = null;
            Assert.Null(AuthUtils.EnvironmentCredentialsFile);

            Assert.Null(AuthUtils.EnvironmentEcsMetaData);
            AuthUtils.EnvironmentEcsMetaData = "test";
            Assert.Equal("test", AuthUtils.EnvironmentEcsMetaData);
            AuthUtils.EnvironmentEcsMetaData = null;

            Assert.Null(AuthUtils.EnvironmentOIDCProviderArn);
            AuthUtils.EnvironmentOIDCProviderArn = "test";
            Assert.Equal("test", AuthUtils.EnvironmentOIDCProviderArn);
            AuthUtils.EnvironmentOIDCProviderArn = null;

            Assert.Null(AuthUtils.EnvironmentOIDCTokenFilePath);
            AuthUtils.EnvironmentOIDCTokenFilePath = TestHelper.GetOIDCTokenFilePath();
            Assert.NotNull(AuthUtils.EnvironmentOIDCTokenFilePath);
            AuthUtils.EnvironmentOIDCTokenFilePath = null;

            Assert.Null(AuthUtils.EnvironmentRoleArn);
            AuthUtils.EnvironmentRoleArn = "test";
            Assert.Equal("test", AuthUtils.EnvironmentRoleArn);
            AuthUtils.EnvironmentRoleArn = null;

            
            AuthUtils.EnvironmentRoleArn = "test";
            AuthUtils.EnvironmentOIDCProviderArn = "test";
            Assert.False(AuthUtils.EnvironmentEnableOIDC());
            AuthUtils.EnvironmentOIDCTokenFilePath = "test";
            Assert.True(AuthUtils.EnvironmentEnableOIDC());
            AuthUtils.EnvironmentRoleArn = null;
            AuthUtils.EnvironmentOIDCProviderArn = null;
            AuthUtils.EnvironmentOIDCTokenFilePath = null;
            Assert.False(AuthUtils.EnvironmentEnableOIDC());
        }
    }
}
