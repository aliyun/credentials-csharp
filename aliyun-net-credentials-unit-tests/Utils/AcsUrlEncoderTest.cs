using System.Text;
using System.Web;

using Aliyun.Credentials.Utils;

using Xunit;

namespace aliyun_net_credentials_unit_tests.Utils
{
    public class AcsUrlEncoderTest
    {
        [Fact]
        public void Encode()
        {
            var source = " ♂:@#¥%&*（";
            var encode = HttpUtility.UrlDecode(AcsURLEncoder.Encode(" ♂:@#¥%&*（"), Encoding.UTF8);
            Assert.Equal(encode, source);
        }

        [Fact]
        public void PercentEncode()
        {
            var result =
                AcsURLEncoder.PercentEncode(
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~!@#$%^&*()");
            Assert.Equal(
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~%21%40%23%24%25%5E%26%2A%28%29",
                result);
        }
    }
}
