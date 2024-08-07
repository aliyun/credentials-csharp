using System.Threading.Tasks;

using Aliyun.Credentials;
using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Provider;
using Aliyun.Credentials.Utils;

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
            mockProvider.Setup(p => p.GetCredentials()).Returns((CredentialModel) null);
            provider.AddCredentialsProvider(mockProvider.Object);
            Assert.Throws<CredentialException>(() => { provider.GetCredentials(); });
            provider.ClearCredentialsProvider();

            mockProvider = new Mock<IAlibabaCloudCredentialsProvider>();
            mockProvider.Setup(p => p.GetCredentials()).Returns(new CredentialModel{
                AccessKeyId = "accessKeyId",
                AccessKeySecret = "accessKeySecret",
                Type = AuthConstant.AccessKey
            });
            provider.AddCredentialsProvider(mockProvider.Object);
            Assert.IsType<CredentialModel>(provider.GetCredentials());

            provider.ClearCredentialsProvider();
            Assert.Throws<CredentialException>(() => { provider.GetCredentials(); });
        }

        [Fact]
        public async Task DefaultProviderAsyncTest()
        {
            DefaultCredentialsProvider provider = new DefaultCredentialsProvider();
            Assert.NotNull(provider);
            await Assert.ThrowsAsync<CredentialException>(async() => { await provider.GetCredentialsAsync(); });

            RamRoleArnCredentialProvider testProvider = new RamRoleArnCredentialProvider("accessKeyId2", "accessKeySecret", "roleArn");
            provider.AddCredentialsProvider(testProvider);
            provider.RemoveCredentialsProvider(testProvider);
            Assert.False(provider.ContainsCredentialsProvider(testProvider));
            provider.ClearCredentialsProvider();

            Mock<IAlibabaCloudCredentialsProvider> mockProvider = new Mock<IAlibabaCloudCredentialsProvider>();
            mockProvider.Setup(p => p.GetCredentialsAsync()).ReturnsAsync((CredentialModel) null);
            provider.AddCredentialsProvider(mockProvider.Object);
            await Assert.ThrowsAsync<CredentialException>(async() => { await provider.GetCredentialsAsync(); });
            provider.ClearCredentialsProvider();

            mockProvider = new Mock<IAlibabaCloudCredentialsProvider>();
            mockProvider.Setup(p => p.GetCredentialsAsync()).ReturnsAsync(new CredentialModel{
                AccessKeyId = "accessKeyId",
                AccessKeySecret = "accessKeySecret",
                Type = AuthConstant.AccessKey
            });
            provider.AddCredentialsProvider(mockProvider.Object);
            Assert.IsType<CredentialModel>(await provider.GetCredentialsAsync());

            provider.ClearCredentialsProvider();
            await Assert.ThrowsAsync<CredentialException>(async() => { await provider.GetCredentialsAsync(); });
        }
    }
}
