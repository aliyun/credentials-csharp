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
            Assert.Equal("accessKeyId", ramRoleArnCredential.AccessKeyId);
            Assert.Equal("accessKeySecret", ramRoleArnCredential.AccessKeySecret);
            Assert.Equal("securityToken", ramRoleArnCredential.SecurityToken);
            Assert.Equal(64090527132000L, ramRoleArnCredential.Expiration);
            Assert.Equal(AuthConstant.RamRoleArn, ramRoleArnCredential.CredentialType);
            Assert.NotNull(ramRoleArnCredential.GetNewCredential());

            ramRoleArnCredential.RefreshCredential();
            Assert.NotNull(ramRoleArnCredential);
        }
    }
}
