using System.Threading.Tasks;

using Aliyun.Credentials;
using Aliyun.Credentials.Provider;
using Aliyun.Credentials.Utils;

using Moq;

using Xunit;

namespace aliyun_net_credentials_unit_tests
{
    public class RamRoleArnCredentialTest
    {
        [Fact]
        public void RamRoleArnTest()
        {
            Mock<IAlibabaCloudCredentialsProvider> mockProvider = new Mock<IAlibabaCloudCredentialsProvider>();
            mockProvider.Setup(p => p.GetCredentials()).Returns(new RamRoleArnCredential("accessKeyId", "accessKeySecret", "securityToken", 10000, null));
            RamRoleArnCredential ramRoleArnCredential = new RamRoleArnCredential("accessKeyId", "accessKeySecret", "securityToken", 64090527132000L, mockProvider.Object);
            Assert.Equal("accessKeyId", ramRoleArnCredential.GetAccessKeyId());
            Assert.Equal("accessKeySecret", ramRoleArnCredential.GetAccessKeySecret());
            Assert.Equal("securityToken", ramRoleArnCredential.GetSecurityToken());
            Assert.Equal(64090527132000L, ramRoleArnCredential.GetExpiration());
            Assert.Equal(AuthConstant.RamRoleArn, ramRoleArnCredential.GetCredentialType());
            Assert.NotNull(ramRoleArnCredential.GetNewCredential());

            ramRoleArnCredential.RefreshCredential();
            Assert.NotNull(ramRoleArnCredential);
        }

        [Fact]
        public async Task RamRoleArnAsyncTest()
        {
            Mock<IAlibabaCloudCredentialsProvider> mockProvider = new Mock<IAlibabaCloudCredentialsProvider>();
            mockProvider.Setup(p => p.GetCredentialsAsync()).ReturnsAsync(new RamRoleArnCredential("accessKeyId", "accessKeySecret", "securityToken", 10000, null));
            RamRoleArnCredential ramRoleArnCredential = new RamRoleArnCredential("accessKeyId", "accessKeySecret", "securityToken", 64090527132000L, mockProvider.Object);
            Assert.Equal("accessKeyId", await ramRoleArnCredential.GetAccessKeyIdAsync());
            Assert.Equal("accessKeySecret", await ramRoleArnCredential.GetAccessKeySecretAsync());
            Assert.Equal("securityToken", await ramRoleArnCredential.GetSecurityTokenAsync());
            Assert.Equal(64090527132000L, await ramRoleArnCredential.GetExpirationAsync());
            Assert.Equal(AuthConstant.RamRoleArn, await ramRoleArnCredential.GetCredentialTypeAsync());
            Assert.NotNull(await ramRoleArnCredential.GetNewCredentialAsync());

            await ramRoleArnCredential.RefreshCredentialAsync();
            Assert.NotNull(ramRoleArnCredential);
        }
    }
}
