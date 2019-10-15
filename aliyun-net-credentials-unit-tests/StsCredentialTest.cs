using Aliyun.Credentials;
using Aliyun.Credentials.Utils;

using Xunit;

namespace aliyun_net_credentials_unit_tests
{
    public class StsCredentialTest
    {
        [Fact]
        public void StsTest()
        {
            StsCredential stsCredential = new StsCredential();
            Assert.NotNull(stsCredential);
            stsCredential = new StsCredential("accessKeyId", "accessKeySecret", "securityToken");
            Assert.Equal("accessKeyId", stsCredential.AccessKeyId);
            Assert.Equal("accessKeySecret", stsCredential.AccessKeySecret);
            Assert.Equal("securityToken", stsCredential.SecurityToken);
            Assert.Equal(AuthConstant.Sts, stsCredential.CredentialType);
        }
    }
}
