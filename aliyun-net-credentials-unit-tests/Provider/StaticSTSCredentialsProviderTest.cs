using System;
using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Provider;
using Xunit;

namespace aliyun_net_credentials_unit_tests.Provider
{
    public class TestStaticSTSCredentialsProvider
    {
        [Fact]
        public void TestConstructor()
        {
            StaticSTSCredentialsProvider provider;
            Config config = new Config();
            var ex = Assert.Throws<ArgumentNullException>(() => provider = new StaticSTSCredentialsProvider(config));
            Assert.StartsWith("SecurityToken must not be null.", ex.Message);
            config.SecurityToken = "securityToken";
            ex = Assert.Throws<ArgumentNullException>(() => provider = new StaticSTSCredentialsProvider(config));
            Assert.StartsWith("AccessKeyId must not be null.", ex.Message);
            config.AccessKeyId = "accessKeyId";
            ex = Assert.Throws<ArgumentNullException>(() => provider = new StaticSTSCredentialsProvider(config));
            Assert.StartsWith("AccessKeySecret must not be null.", ex.Message);
            config.AccessKeySecret = "accessKeySecret";
            provider = new StaticSTSCredentialsProvider(config);
            Assert.NotNull(provider);
            Assert.Equal("static_sts", provider.GetProviderName());
            CredentialModel credentialModel = provider.GetCredentials();            
            Assert.Equal("accessKeyId", credentialModel.AccessKeyId);
            Assert.Equal("accessKeySecret", credentialModel.AccessKeySecret);
            Assert.Equal("securityToken", credentialModel.SecurityToken);
        }
    }
}