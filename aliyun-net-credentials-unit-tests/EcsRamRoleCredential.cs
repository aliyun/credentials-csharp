using System.Threading.Tasks;

using Aliyun.Credentials;
using Aliyun.Credentials.Provider;
using Aliyun.Credentials.Utils;

using Moq;

using Xunit;

namespace aliyun_net_credentials_unit_tests
{
    public class EcsRamRoleCredentialTest
    {
        [Fact]
        public void TestEcsRamRoleCredential()
        {
            Mock<IAlibabaCloudCredentialsProvider> mockProvider = new Mock<IAlibabaCloudCredentialsProvider>();
            mockProvider.Setup(p => p.GetCredentials()).Returns(new EcsRamRoleCredential("accessKeyId", "accessKeySecret", "securityToken", 64090527132000L, null));
            EcsRamRoleCredential ecsRamRoleCredential = new EcsRamRoleCredential("accessKeyId", "accessKeySecret", "securityToken", 64090527132000L, mockProvider.Object);
            Assert.Equal("accessKeyId", ecsRamRoleCredential.GetAccessKeyId());
            Assert.Equal("accessKeySecret", ecsRamRoleCredential.GetAccessKeySecret());
            Assert.Equal("securityToken", ecsRamRoleCredential.GetSecurityToken());
            Assert.Equal(64090527132000L, ecsRamRoleCredential.GetExpiration());
            Assert.Equal(AuthConstant.EcsRamRole, ecsRamRoleCredential.GetCredentialType());
            Assert.NotNull(ecsRamRoleCredential.GetNewCredential<EcsRamRoleCredential>());
        }

        [Fact]
        public async Task TestEcsRamRoleCredentialAsync()
        {
            Mock<IAlibabaCloudCredentialsProvider> mockProvider = new Mock<IAlibabaCloudCredentialsProvider>();
            mockProvider.Setup(p => p.GetCredentialsAsync()).ReturnsAsync(new EcsRamRoleCredential("accessKeyId", "accessKeySecret", "securityToken", 64090527132000L, null));
            EcsRamRoleCredential ecsRamRoleCredential = new EcsRamRoleCredential("accessKeyId", "accessKeySecret", "securityToken", 64090527132000L, mockProvider.Object);

            Assert.Equal("accessKeyId", await ecsRamRoleCredential.GetAccessKeyIdAsync());
            Assert.Equal("accessKeySecret", await ecsRamRoleCredential.GetAccessKeySecretAsync());
            Assert.Equal("securityToken", await ecsRamRoleCredential.GetSecurityTokenAsync());
            Assert.Equal(64090527132000L, await ecsRamRoleCredential.GetExpirationAsync());
            Assert.Equal(AuthConstant.EcsRamRole, await ecsRamRoleCredential.GetCredentialTypeAsync());
            Assert.NotNull(await ecsRamRoleCredential.GetNewCredentialAsync<EcsRamRoleCredential>());
        }

        [Theory]
        [InlineData(64090527132000L)]
        [InlineData(0L)]
        public void TestEcsRamRoleCredentialShouldRefresh(long expiration)
        {
            Mock<IAlibabaCloudCredentialsProvider> mockProvider = new Mock<IAlibabaCloudCredentialsProvider>();
            mockProvider.Setup(p => p.GetCredentials()).Returns(new EcsRamRoleCredential("accessKeyId", "accessKeySecret", "securityToken", 64090527132000L, null));
            EcsRamRoleCredential ecsRamRoleCredential = new EcsRamRoleCredential("accessKeyId", "accessKeySecret", "securityToken", expiration, mockProvider.Object);
            if (expiration == 0)
            {
                Assert.True(ecsRamRoleCredential.WithShouldRefresh());
            }
            else
            {
                Assert.False(ecsRamRoleCredential.WithShouldRefresh());
            }
            ecsRamRoleCredential.RefreshCredential();
            Assert.NotNull(ecsRamRoleCredential);
        }

        [Theory]
        [InlineData(64090527132000L)]
        [InlineData(0L)]
        public async Task TestEcsRamRoleCredentialShouldRefreshAsync(long expiration)
        {
            Mock<IAlibabaCloudCredentialsProvider> mockProvider = new Mock<IAlibabaCloudCredentialsProvider>();
            mockProvider.Setup(p => p.GetCredentialsAsync()).ReturnsAsync(new EcsRamRoleCredential("accessKeyId", "accessKeySecret", "securityToken", 64090527132000L, null));
            EcsRamRoleCredential ecsRamRoleCredential = new EcsRamRoleCredential("accessKeyId", "accessKeySecret", "securityToken", expiration, mockProvider.Object);
            if (expiration == 0)
            {
                Assert.True(ecsRamRoleCredential.WithShouldRefresh());
            }
            else
            {
                Assert.False(ecsRamRoleCredential.WithShouldRefresh());
            }
            await ecsRamRoleCredential.RefreshCredentialAsync();
            Assert.NotNull(ecsRamRoleCredential);
        }
    }
}
