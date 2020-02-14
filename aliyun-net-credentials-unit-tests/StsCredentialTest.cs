using System.Threading.Tasks;

using Aliyun.Credentials;
using Aliyun.Credentials.Utils;

using Xunit;

namespace aliyun_net_credentials_unit_tests
{
    public class StsCredentialTest
    {
        [Fact]
        public async Task StsTest()
        {
            StsCredential stsCredential = new StsCredential();
            Assert.NotNull(stsCredential);
            stsCredential = new StsCredential("accessKeyId", "accessKeySecret", "securityToken");
            Assert.Equal("accessKeyId", stsCredential.GetAccessKeyId());
            Assert.Equal("accessKeySecret", stsCredential.GetAccessKeySecret());
            Assert.Equal("securityToken", stsCredential.GetSecurityToken());
            Assert.Equal(AuthConstant.Sts, stsCredential.GetCredentialType());

            Assert.Equal("accessKeyId", await stsCredential.GetAccessKeyIdAsync());
            Assert.Equal("accessKeySecret", await stsCredential.GetAccessKeySecretAsync());
            Assert.Equal("securityToken", await stsCredential.GetSecurityTokenAsync());
            Assert.Equal(AuthConstant.Sts, await stsCredential.GetCredentialTypeAsync());
        }
    }
}
