using System.IO;
using System.Threading.Tasks;

using Aliyun.Credentials;
using Aliyun.Credentials.Utils;

using Xunit;

namespace aliyun_net_credentials_unit_tests
{
    public class RsaKeyPairCredentialTest
    {
        [Fact]
        public async Task RsaKeyPairTest()
        {
            RsaKeyPairCredential rsaKeyPairCredential = new RsaKeyPairCredential("publicKeyId", "privateKeySecret", 10000, null);
            Assert.Equal("publicKeyId", rsaKeyPairCredential.GetAccessKeyId());
            Assert.Equal("privateKeySecret", rsaKeyPairCredential.GetAccessKeySecret());
            Assert.Null(rsaKeyPairCredential.GetSecurityToken());
            Assert.Equal(10000, rsaKeyPairCredential.GetExpiration());
            Assert.Equal(AuthConstant.RsaKeyPair, rsaKeyPairCredential.GetCredentialType());

            Assert.Equal("publicKeyId", await rsaKeyPairCredential.GetAccessKeyIdAsync());
            Assert.Equal("privateKeySecret", await rsaKeyPairCredential.GetAccessKeySecretAsync());
            Assert.Null(await rsaKeyPairCredential.GetSecurityTokenAsync());
            Assert.Equal(10000, await rsaKeyPairCredential.GetExpirationAsync());
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
