using Aliyun.Credentials;

using Xunit;

namespace aliyun_net_credentials_unit_tests
{
    public class ConfigurationTest
    {
        [Fact]
        public void GetConfig()
        {
            Configuration config = new Configuration();
            Assert.NotNull(config);

            config.AccessKeyId = "AccessKeyId";
            Assert.Equal("AccessKeyId", config.AccessKeyId);

            config.AccessKeySecret = "AccessKeySecret";
            Assert.Equal("AccessKeySecret", config.AccessKeySecret);

            config.CertFile = "CertFile";
            Assert.Equal("CertFile", config.CertFile);

            config.CertPassword = "CertPassword";
            Assert.Equal("CertPassword", config.CertPassword);

            config.ConnectTimeout = 10000;
            Assert.Equal(10000, config.ConnectTimeout);

            config.Host = "Host";
            Assert.Equal("Host", config.Host);

            config.PrivateKeyFile = "PrivateKeyFile";
            Assert.Equal("PrivateKeyFile", config.PrivateKeyFile);

            config.Proxy = "Proxy";
            Assert.Equal("Proxy", config.Proxy);

            config.PublicKeyId = "PublicKeyId";
            Assert.Equal("PublicKeyId", config.PublicKeyId);

            config.ReadTimeout = 20000;
            Assert.Equal(20000, config.ReadTimeout);

            config.RoleArn = "RoleArn";
            Assert.Equal("RoleArn", config.RoleArn);

            config.RoleName = "RoleName";
            Assert.Equal("RoleName", config.RoleName);

            config.RoleSessionName = "RoleSessionName";
            Assert.Equal("RoleSessionName", config.RoleSessionName);

            config.SecurityToken = "SecurityToken";
            Assert.Equal("SecurityToken", config.SecurityToken);

            config.Type = "Type";
            Assert.Equal("Type", config.Type);
        }
    }
}
