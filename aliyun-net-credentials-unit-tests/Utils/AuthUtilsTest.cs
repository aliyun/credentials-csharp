using Aliyun.Credentials.Utils;

using Xunit;

namespace aliyun_net_credentials_unit_tests.Utils
{
    public class AuthUtilsTest
    {
        [Fact]
        public void GetPrivateKeyTest()
        {
            string privatKey = AuthUtils.GetPrivateKey(TestHelper.GetIniFilePath());

            Assert.NotNull(privatKey);
            Assert.NotEmpty(privatKey);
        }
    }
}
