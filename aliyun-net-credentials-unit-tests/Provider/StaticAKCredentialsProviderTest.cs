using System;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Provider;
using Xunit;

namespace aliyun_net_credentials_unit_tests.Provider
{
    public class TestStaticAKCredentialsProvider
    {
        [Fact]
        public void TestConstructor()
        {
            StaticAKCredentialsProvider provider;
            Config config = new Config();
            var ex = Assert.Throws<ArgumentNullException>(() => provider = new StaticAKCredentialsProvider(config));
            Assert.StartsWith("AccessKeyId must not be null.", ex.Message);
            config.AccessKeyId = "accessKeyId";
            ex = Assert.Throws<ArgumentNullException>(() => provider = new StaticAKCredentialsProvider(config));
            Assert.StartsWith("AccessKeySecret must not be null.", ex.Message);
            config.AccessKeySecret = "accessKeySecret";
            provider = new StaticAKCredentialsProvider(config);
            Assert.NotNull(provider);
            Assert.Equal("static_ak", provider.GetProviderName());
            CredentialModel credentialModel = provider.GetCredentials();            
            Assert.Equal("accessKeyId", credentialModel.AccessKeyId);
            Assert.Equal("accessKeySecret", credentialModel.AccessKeySecret);
            Assert.Equal("static_ak", credentialModel.ProviderName);
        }
    }
}