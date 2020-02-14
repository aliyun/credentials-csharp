using System;
using System.Threading.Tasks;
using Aliyun.Credentials;
using Aliyun.Credentials.Utils;

using Xunit;

namespace aliyun_net_credentials_unit_tests
{
    public class AccessKeyCredentialTest
    {
        [Fact]
        public async Task TesetAccessKeyCredential()
        {
            AccessKeyCredential accessKeyCredential;
            try
            {
                Assert.Throws<ArgumentNullException>(() => new AccessKeyCredential(null, "test"));
            }
            catch (ArgumentNullException e)
            {
                Assert.Equal("Access key ID cannot be null.", e.Message);
            }

            try
            {
                Assert.Throws<ArgumentNullException>(() => new AccessKeyCredential("test", null));
            }
            catch (ArgumentNullException e)
            {
                Assert.Equal("Access key secret cannot be null.", e.Message);
            }

            accessKeyCredential = new AccessKeyCredential("accessKeyId", "accessKeySecret");
            Assert.NotNull(accessKeyCredential);
            Assert.Equal("accessKeyId", accessKeyCredential.GetAccessKeyId());
            Assert.Equal("accessKeySecret", accessKeyCredential.GetAccessKeySecret());
            Assert.Null(accessKeyCredential.GetSecurityToken());
            Assert.Equal(AuthConstant.AccessKey, accessKeyCredential.GetCredentialType());
            Assert.Equal("accessKeyId", await accessKeyCredential.GetAccessKeyIdAsync());
            Assert.Equal("accessKeySecret", await accessKeyCredential.GetAccessKeySecretAsync());
            Assert.Null(await accessKeyCredential.GetSecurityTokenAsync());
            Assert.Equal(AuthConstant.AccessKey, await accessKeyCredential.GetCredentialTypeAsync());
        }
    }
}
