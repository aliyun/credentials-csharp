using Aliyun.Credentials.Utils;

using Xunit;

namespace aliyun_net_credentials_unit_tests.Utils
{
    public class IniFileHelperTest
    {
        [Fact]
        public void IniFileTest()
        {
            var iniReader = new IniFileHelper(TestHelper.GetIniFilePath());
            var result = iniReader.GetKeys("default");
            Assert.True(0 < result.Length);
            Assert.NotNull(iniReader.GetSections());

            var resultNotExist = iniReader.GetKeys("notExist");
            Assert.Equal(new string[0], resultNotExist);
            Assert.Equal("", iniReader.GetValue("type"));
            Assert.Equal("", iniReader.GetValue("type", "notExist"));
            Assert.Equal("", iniReader.GetValue("noType", "notExist"));
            Assert.Equal("access_key", iniReader.GetValue("type", "default"));
        }

    }
}
