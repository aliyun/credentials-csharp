using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Aliyun.Credentials.Http;
using Aliyun.Credentials.Utils;

using Xunit;

namespace aliyun_net_credentials_unit_tests.Utils
{
    public class ParameterHelperTest
    {
        public DateTime dateTime()
        {
            DateTime datetime;
            var timeStamp = 1548311719318;
            var startTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            datetime = startTime.AddMilliseconds(timeStamp);
            datetime = TimeZoneInfo.ConvertTimeToUtc(datetime);
            return datetime;
        }

        [Fact]
        public void FormatIso8601Date()
        {
            var datetime = dateTime();
            var result = ParameterHelper.FormatIso8601Date(datetime);
            Assert.NotEqual(datetime.ToString(CultureInfo.InvariantCulture), result);
            Assert.Equal("2019-01-24T06:35:19Z", result);
        }

        [Fact]
        public void FormatTypeToString()
        {
            Assert.Equal("application/xml", ParameterHelper.FormatTypeToString(FormatType.Xml));
            Assert.Equal("application/json", ParameterHelper.FormatTypeToString(FormatType.Json));
            Assert.Equal("application/x-www-form-urlencoded", ParameterHelper.FormatTypeToString(FormatType.Form));
            Assert.Equal("application/octet-stream", ParameterHelper.FormatTypeToString(FormatType.Raw));
        }

        [Fact]
        public void GetRFC2616Date()
        {
            var datetime = dateTime();
            var result = ParameterHelper.GetRfc2616Date(datetime);
            Assert.Equal("Thu, 24 Jan 2019 06:35:19 GMT", result);
        }

        [Fact]
        public void Md5Sum()
        {
            var str = "md5 sum";
            var buff = Encoding.Default.GetBytes(str);
            var result = ParameterHelper.Md5Sum(buff);
            Assert.Equal("018A7FC7456F40EE0D083CFCBF1EE472", result);
        }

        [Fact]
        public void Md5SumAndBase64()
        {
            var str = "md5 sum";
            var buff = Encoding.Default.GetBytes(str);
            var result = ParameterHelper.Md5SumAndBase64(buff);
            Assert.Equal("AYp/x0VvQO4NCDz8vx7kcg==", result);
        }

        [Fact]
        public void StingToFormatType()
        {
            Assert.Equal("application/xml",
                ParameterHelper.FormatTypeToString(ParameterHelper.StingToFormatType("application/xml"))
            );
            Assert.Equal("application/xml",
                ParameterHelper.FormatTypeToString(ParameterHelper.StingToFormatType("text/xml"))
            );
            Assert.Equal("application/json",
                ParameterHelper.FormatTypeToString(ParameterHelper.StingToFormatType("application/json"))
            );
            Assert.Equal("application/x-www-form-urlencoded",
                ParameterHelper.FormatTypeToString(
                    ParameterHelper.StingToFormatType("application/x-www-form-urlencoded"))
            );

            Assert.Equal(FormatType.Raw, ParameterHelper.StingToFormatType("raw"));
        }

        [Fact]
        public void StringToMethodType()
        {
            Assert.True(MethodType.Get == ParameterHelper.StringToMethodType("get"));
            Assert.True(MethodType.Post == ParameterHelper.StringToMethodType("post"));
            Assert.True(MethodType.Delete == ParameterHelper.StringToMethodType("delete"));
            Assert.True(MethodType.Put == ParameterHelper.StringToMethodType("put"));
            Assert.True(MethodType.Head == ParameterHelper.StringToMethodType("head"));
            Assert.True(MethodType.Options == ParameterHelper.StringToMethodType("options"));
            Assert.True(null == ParameterHelper.StringToMethodType("test"));
        }

        [Fact]
        public void ComposeUrlTest()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("key", "value");
            dic.Add("keyNull", null);
            Assert.NotNull(ParameterHelper.ComposeUrl("www.aliyun.com", dic, "http"));
        }

        [Fact]
        public void TestValidateNotNull()
        {
            string str = null;
            var ex = Assert.Throws<ArgumentNullException>(() => ParameterHelper.ValidateNotNull(str, "str", "str must not be null."));
            Assert.StartsWith("str must not be null.", ex.Message);
        }
    }
}
