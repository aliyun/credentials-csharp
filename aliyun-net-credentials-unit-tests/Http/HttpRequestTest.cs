using System.Collections.Generic;

using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Http;

using Xunit;

namespace aliyun_net_credentials_unit_tests.Http
{
    public class HttpRequestTest
    {
        [Fact]
        public void HttpRequestParamTest()
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.SetCommonUrlParameters();
            httpRequest.AddUrlParameter("key", "value");
            Assert.Equal("value", httpRequest.UrlParameters["key"]);
            Assert.Equal(5, httpRequest.UrlParameters.Count);

            httpRequest.Headers.Add("testHeader", "test");
            Assert.Equal("test", httpRequest.Headers["testHeader"]);

            httpRequest.ConnectTimeout = 10000;
            Assert.Equal(10000, httpRequest.ConnectTimeout);

            httpRequest.Content = new byte[] { };
            Assert.NotNull(httpRequest.GetHttpContentString());

            httpRequest.ContentType = FormatType.Json;
            Assert.Equal(FormatType.Json, httpRequest.ContentType);

            httpRequest.Encoding = "UTF-8";
            Assert.Equal("UTF-8", httpRequest.Encoding);

            httpRequest.Method = MethodType.Post;
            Assert.Equal(MethodType.Post, httpRequest.Method);

            httpRequest.ReadTimeout = 20000;
            Assert.Equal(20000, httpRequest.ReadTimeout);

            httpRequest.Url = "www.test.com";
            Assert.Equal("www.test.com", httpRequest.Url);

        }

        [Fact]
        public void GetHttpContentStringTest()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("testKey", "testValue");
            HttpRequest httpRequest = new HttpRequest("http://www.aliyun.com", headers);
            httpRequest.Encoding = "test";
            httpRequest.Content = new byte[] { };
            try
            {
                Assert.Throws<CredentialException>(() => { httpRequest.GetHttpContentString(); });
            }
            catch (CredentialException e)
            {
                Assert.Equal("Can not parse response due to unsupported encoding.", e.Message);
            }
        }

        [Fact]
        public void GetHttpRequestTest()
        {
            HttpRequest httpRequest = new HttpRequest("http://www.aliyun.com", null);
            Assert.NotNull(httpRequest);
        }
    }
}
