using System;
using System.IO;
using System.Threading.Tasks;

using Aliyun.Credentials;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Provider;
using Aliyun.Credentials.Utils;

using Moq;

using Xunit;

namespace aliyun_net_credentials_unit_tests
{
    public class RsaKeyPairCredentialTest
    {
        [Fact]
        public async Task TestWithShouldRefresh()
        {
            var mockProvider = new Mock<IAlibabaCloudCredentialsProvider>();
            var credentialModel = new CredentialModel
            {
                AccessKeyId = "newPublicKeyId",
                AccessKeySecret = "newPrivateKeySecret",
                Expiration = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeMilliseconds()
            };
            mockProvider.Setup(p => p.GetCredentials()).Returns(credentialModel);
            mockProvider.Setup(p => p.GetCredentialsAsync()).ReturnsAsync(credentialModel);

            // 凭证过期
            var staleExpiration = DateTimeOffset.UtcNow.AddMinutes(-5).ToUnixTimeMilliseconds();
            var rsaKeyPairCredential = new RsaKeyPairCredential("publicKeyId", "privateKeySecret", staleExpiration, mockProvider.Object);

            rsaKeyPairCredential.RefreshCredential();
            var accessKeyId = rsaKeyPairCredential.GetAccessKeyId();
            var accessKeySecret = rsaKeyPairCredential.GetAccessKeySecret();
            var expiration = rsaKeyPairCredential.GetExpiration();

            Assert.Equal("newPublicKeyId", accessKeyId);
            Assert.Equal("newPrivateKeySecret", accessKeySecret);
            Assert.Equal(credentialModel.Expiration, expiration);


            var rsaKeyPairCredential1 = new RsaKeyPairCredential("publicKeyId", "privateKeySecret", staleExpiration, mockProvider.Object);
            await rsaKeyPairCredential1.RefreshCredentialAsync();
            accessKeyId = await rsaKeyPairCredential1.GetAccessKeyIdAsync();
            accessKeySecret = await rsaKeyPairCredential1.GetAccessKeySecretAsync();
            expiration = await rsaKeyPairCredential1.GetExpirationAsync();

            Assert.Equal("newPublicKeyId", accessKeyId);
            Assert.Equal("newPrivateKeySecret", accessKeySecret);
            Assert.Equal(credentialModel.Expiration, expiration);

            mockProvider.Verify(p => p.GetCredentials(), Times.Once);
            mockProvider.Verify(p => p.GetCredentialsAsync(), Times.Once);
        }

        [Fact]
        public async Task RsaKeyPairTest()
        {
            Mock<IAlibabaCloudCredentialsProvider> mockProvider = new Mock<IAlibabaCloudCredentialsProvider>();
            mockProvider.Setup(p => p.GetCredentials()).Returns(new CredentialModel
            {
                AccessKeyId = "publicKeyId",
                AccessKeySecret = "privateKeySecret",
                Expiration = 1000,
            });
            mockProvider.Setup(p => p.GetCredentialsAsync()).ReturnsAsync(new CredentialModel
            {
                AccessKeyId = "publicKeyId",
                AccessKeySecret = "privateKeySecret",
                Expiration = 1000,
            });
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
