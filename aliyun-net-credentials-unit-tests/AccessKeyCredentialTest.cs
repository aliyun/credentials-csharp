using System;

using Aliyun.Credentials;
using Aliyun.Credentials.Utils;

using Xunit;

namespace aliyun_net_credentials_unit_tests
{
    public class AccessKeyCredentialTest
    {
        [Fact]
        public void TesetAccessKeyCredential()
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
            Assert.Equal("accessKeyId", accessKeyCredential.AccessKeyId);
            Assert.Equal("accessKeySecret", accessKeyCredential.AccessKeySecret);
            Assert.Null(accessKeyCredential.SecurityToken);
            Assert.Equal(AuthConstant.AccessKey, accessKeyCredential.CredentialType);
        }
    }
}
