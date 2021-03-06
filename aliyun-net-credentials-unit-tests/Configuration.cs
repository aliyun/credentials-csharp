using Aliyun.Credentials;
using Aliyun.Credentials.Models;

using Xunit;

namespace aliyun_net_credentials_unit_tests
{
    public class ConfigurationTest
    {
        [Fact]
        public void GetConfig()
        {
            Config config = new Config();
            Assert.NotNull(config);

            config.AccessKeyId = "AccessKeyId";
            Assert.Equal("AccessKeyId", config.AccessKeyId);

            config.AccessKeySecret = "AccessKeySecret";
            Assert.Equal("AccessKeySecret", config.AccessKeySecret);

            config.ConnectTimeout = 10000;
            Assert.Equal(10000, config.ConnectTimeout);

            config.BearerToken = "bearerToken";
            Assert.Equal("bearerToken", config.BearerToken);

            config.Host = "Host";
            Assert.Equal("Host", config.Host);

            config.PrivateKeyFile = "PrivateKeyFile";
            Assert.Equal("PrivateKeyFile", config.PrivateKeyFile);

            config.Proxy = "Proxy";
            Assert.Equal("Proxy", config.Proxy);

            config.PublicKeyId = "PublicKeyId";
            Assert.Equal("PublicKeyId", config.PublicKeyId);

            config.Timeout = 20000;
            Assert.Equal(20000, config.Timeout);

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
