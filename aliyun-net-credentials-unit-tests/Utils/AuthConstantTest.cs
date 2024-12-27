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

        [Fact]
        public void GetHomePathTest()
        {
            var path = TestHelper.RunStaticMethodWithReturn(typeof(AuthConstant), "GetHomePath",
                new object[] { });
            Assert.NotEmpty((string)path);
        }
        
        [Fact]
        public void GetSlashTest()
        {
            var path = TestHelper.RunStaticMethodWithReturn(typeof(AuthConstant), "GetOsSlash",
                new object[] { });
            Assert.True(((string)path).Equals("/") || ((string)path).Equals("\\"));
        }
    }
}