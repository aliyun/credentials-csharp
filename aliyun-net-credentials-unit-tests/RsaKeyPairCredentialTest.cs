using System.IO;
using System.Threading.Tasks;

using Aliyun.Credentials;
using Aliyun.Credentials.Provider;
using Aliyun.Credentials.Utils;

using Moq;

using Xunit;

namespace aliyun_net_credentials_unit_tests
{
    public class RsaKeyPairCredentialTest
    {
        [Fact]
        public async Task RsaKeyPairTest()
        {
            Mock<IAlibabaCloudCredentialsProvider> mockProvider = new Mock<IAlibabaCloudCredentialsProvider>();
            mockProvider.Setup(p => p.GetCredentials()).Returns(new RsaKeyPairCredential("publicKeyId", "privateKeySecret", 1000, null));
            mockProvider.Setup(p => p.GetCredentialsAsync()).ReturnsAsync(new RsaKeyPairCredential("publicKeyId", "privateKeySecret", 1000, null));
            RsaKeyPairCredential rsaKeyPairCredential = new RsaKeyPairCredential("publicKeyId", "privateKeySecret", 64090527132000L, mockProvider.Object);
            Assert.Equal("publicKeyId", rsaKeyPairCredential.GetAccessKeyId());
            Assert.Equal("privateKeySecret", rsaKeyPairCredential.GetAccessKeySecret());
            Assert.Null(rsaKeyPairCredential.GetSecurityToken());
            Assert.Equal(64090527132000, rsaKeyPairCredential.GetExpiration());
            Assert.Equal(AuthConstant.RsaKeyPair, rsaKeyPairCredential.GetCredentialType());

            Assert.Equal("publicKeyId", await rsaKeyPairCredential.GetAccessKeyIdAsync());
            Assert.Equal("privateKeySecret", await rsaKeyPairCredential.GetAccessKeySecretAsync());
            Assert.Null(await rsaKeyPairCredential.GetSecurityTokenAsync());
            Assert.Equal(64090527132000, await rsaKeyPairCredential.GetExpirationAsync());
            Assert.Equal(AuthConstant.RsaKeyPair, await rsaKeyPairCredential.GetCredentialTypeAsync());
        }

        [Fact]
        public void RsaKeyPairTestNull()
        {
            Assert.Equal("You must provide a valid pair of Public Key ID and Private Key Secret.",
                Assert.Throws<InvalidDataException>(() =>
                {
                    RsaKeyPairCredential unused = new RsaKeyPairCredential(null, "privateKeySecret", 10000, null);
                }).Message
            );

            Assert.Equal("You must provide a valid pair of Public Key ID and Private Key Secret.",
                Assert.Throws<InvalidDataException>(() =>
                {
                    RsaKeyPairCredential unused = new RsaKeyPairCredential("publicKeyId", null, 10000, null);
                }).Message
            );
        }
    }
}
