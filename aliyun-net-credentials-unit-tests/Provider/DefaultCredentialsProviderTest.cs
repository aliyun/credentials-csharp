using Aliyun.Credentials;
using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Provider;

using Moq;

using Xunit;

namespace aliyun_net_credentials_unit_tests.Provider
{
    public class DefaultCredentialsProviderTest
    {
        [Fact]
        public void DefaultProviderTest()
        {
            DefaultCredentialsProvider provider = new DefaultCredentialsProvider();
            Assert.NotNull(provider);
            Assert.Throws<CredentialException>(() => { provider.GetCredentials(); });

            RamRoleArnCredentialProvider testProvider = new RamRoleArnCredentialProvider("accessKeyId2", "accessKeySecret", "roleArn");
            provider.AddCredentialsProvider(testProvider);
            provider.RemoveCredentialsProvider(testProvider);
            Assert.False(provider.ContainsCredentialsProvider(testProvider));
            provider.ClearCredentialsProvider();

            Mock<IAlibabaCloudCredentialsProvider> mockProvider = new Mock<IAlibabaCloudCredentialsProvider>();
            mockProvider.Setup(p => p.GetCredentials()).Returns((AccessKeyCredential) null);
            provider.AddCredentialsProvider(mockProvider.Object);
            Assert.Throws<CredentialException>(() => { provider.GetCredentials(); });
            provider.ClearCredentialsProvider();

            mockProvider = new Mock<IAlibabaCloudCredentialsProvider>();
            mockProvider.Setup(p => p.GetCredentials()).Returns(new AccessKeyCredential("accessKeyId", "accessKeySecret"));
            provider.AddCredentialsProvider(mockProvider.Object);
            Assert.IsType<AccessKeyCredential>(provider.GetCredentials());

            provider.ClearCredentialsProvider();
            Assert.Throws<CredentialException>(() => { provider.GetCredentials(); });
        }
    }
}
