using System;

using Aliyun.Credentials.Utils;

using Xunit;

namespace aliyun_net_credentials_unit_tests.Utils
{
    public class DateTimeExtensionsTest
    {
        [Fact]
        public void TestCurrentTimeMillis()
        {
            var d = DateTime.UtcNow;
            var r = d.GetTimeMillis();
            Assert.IsType<long>(r);
        }
    }
}
