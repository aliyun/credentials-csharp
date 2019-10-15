using Aliyun.Credentials.Utils;

using Xunit;

namespace aliyun_net_credentials_unit_tests.Utils
{
    public class AuthConstantTest
    {
        [Fact]
        public void GetDefaultFilePathTest()
        {
            string path = AuthConstant.GetDefaultFilePath();
            Assert.NotEmpty(path);
        }
    }
}
