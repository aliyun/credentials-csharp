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
            Assert.Equal("accessKeyId", ecsRamRoleCredential.AccessKeyId);
            Assert.Equal("accessKeySecret", ecsRamRoleCredential.AccessKeySecret);
            Assert.Equal("securityToken", ecsRamRoleCredential.SecurityToken);
            Assert.Equal(64090527132000L, ecsRamRoleCredential.Expiration);
            Assert.Equal(AuthConstant.EcsRamRole, ecsRamRoleCredential.CredentialType);
            Assert.NotNull(ecsRamRoleCredential.GetNewCredential());
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
        }
    }
}
