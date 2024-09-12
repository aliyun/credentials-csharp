using System;
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
            var cacheClientType = AuthUtils.ClientType;
            var cacheEnvironmentAccessKeyId = AuthUtils.EnvironmentAccessKeyId;
            var cacheEnvironmentAccesskeySecret = AuthUtils.EnvironmentAccesskeySecret;
            var cacheEnvironmentCredentialsFile = AuthUtils.EnvironmentCredentialsFile;
            var cacheEnvironmentEcsMetaData = AuthUtils.EnvironmentEcsMetaData;
            var cacheEnvironmentOIDCProviderArn = AuthUtils.EnvironmentOIDCProviderArn;
            var cacheEnvironmentOIDCTokenFilePath = AuthUtils.EnvironmentOIDCTokenFilePath;
            var cacheEnvironmentRoleArn = AuthUtils.EnvironmentRoleArn;

            AuthUtils.ClientType = "test";
            Assert.Equal("test", AuthUtils.ClientType);
            AuthUtils.ClientType = cacheClientType;

            AuthUtils.EnvironmentAccessKeyId = "test";
            Assert.Equal("test", AuthUtils.EnvironmentAccessKeyId);
            Environment.SetEnvironmentVariable("ALIBABA_CLOUD_ACCESS_KEY_ID", null);
            AuthUtils.EnvironmentAccessKeyId = cacheEnvironmentAccessKeyId;

            AuthUtils.EnvironmentAccesskeySecret = "test";
            Assert.Equal("test", AuthUtils.EnvironmentAccesskeySecret);
            AuthUtils.EnvironmentAccesskeySecret = cacheEnvironmentAccesskeySecret;

            AuthUtils.EnvironmentCredentialsFile = "test";
            Assert.Equal("test", AuthUtils.EnvironmentCredentialsFile);
            AuthUtils.EnvironmentCredentialsFile = cacheEnvironmentCredentialsFile;

            AuthUtils.EnvironmentEcsMetaData = "test";
            Assert.Equal("test", AuthUtils.EnvironmentEcsMetaData);
            AuthUtils.EnvironmentEcsMetaData = cacheEnvironmentEcsMetaData;

            AuthUtils.EnvironmentOIDCProviderArn = "test";
            Assert.Equal("test", AuthUtils.EnvironmentOIDCProviderArn);
            AuthUtils.EnvironmentOIDCProviderArn = cacheEnvironmentOIDCProviderArn;

            AuthUtils.EnvironmentOIDCTokenFilePath = TestHelper.GetOIDCTokenFilePath();
            Assert.NotNull(AuthUtils.EnvironmentOIDCTokenFilePath);
            AuthUtils.EnvironmentOIDCTokenFilePath = cacheEnvironmentOIDCTokenFilePath;

            AuthUtils.EnvironmentRoleArn = "test";
            Assert.Equal("test", AuthUtils.EnvironmentRoleArn);
            AuthUtils.EnvironmentOIDCProviderArn = "test";
            Assert.False(AuthUtils.EnvironmentEnableOIDC());
            AuthUtils.EnvironmentOIDCTokenFilePath = "test";
            Assert.True(AuthUtils.EnvironmentEnableOIDC());
            AuthUtils.EnvironmentRoleArn = cacheEnvironmentRoleArn;
            AuthUtils.EnvironmentOIDCProviderArn = cacheEnvironmentOIDCProviderArn;
            AuthUtils.EnvironmentOIDCTokenFilePath = cacheEnvironmentOIDCTokenFilePath;
        }
    }
}
