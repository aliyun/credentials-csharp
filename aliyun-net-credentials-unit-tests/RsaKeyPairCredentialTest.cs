using System.IO;

using Aliyun.Credentials;
using Aliyun.Credentials.Utils;

using Xunit;

namespace aliyun_net_credentials_unit_tests
{
    public class RsaKeyPairCredentialTest
    {
        [Fact]
        public void RsaKeyPairTest()
        {
            RsaKeyPairCredential rsaKeyPairCredential = new RsaKeyPairCredential("publicKeyId", "privateKeySecret", 10000, null);
            Assert.Equal("publicKeyId", rsaKeyPairCredential.AccessKeyId);
            Assert.Equal("privateKeySecret", rsaKeyPairCredential.AccessKeySecret);
            Assert.Null(rsaKeyPairCredential.SecurityToken);
            Assert.Equal(10000, rsaKeyPairCredential.Expiration);
            Assert.Equal(AuthConstant.RsaKeyPair, rsaKeyPairCredential.CredentialType);
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
