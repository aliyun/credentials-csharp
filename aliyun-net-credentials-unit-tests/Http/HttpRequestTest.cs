using System.Collections.Generic;
using System.Net.Cache;
using System.Text;
using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Http;
using Aliyun.Credentials.Utils;
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

            httpRequest.Method = MethodType.POST;
            Assert.Equal(MethodType.POST, httpRequest.Method);

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

        [Fact]
        public void SetHttpContentTest()
        {
            // null content
            HttpRequest httpRequest = new HttpRequest("test");
            httpRequest.Method = MethodType.POST;
            httpRequest.SetHttpContent(null, null, null);
            Assert.Null(DictionaryUtil.Get(httpRequest.Headers, "Content-MD5"));
            Assert.Null(httpRequest.Content);
            Assert.Null(httpRequest.ContentType);
            Assert.Null(httpRequest.Encoding);
            Assert.Equal("0", DictionaryUtil.Get(httpRequest.Headers, "Content-Length"));

            // GET test
            httpRequest.Method = MethodType.GET;
            httpRequest.SetHttpContent(Encoding.UTF8.GetBytes("content"), null, null);
            Assert.Equal("1B2M2Y8AsgTpgAmY7PhCfg==", DictionaryUtil.Get(httpRequest.Headers, "Content-MD5"));

            // POST test
            httpRequest.Method = MethodType.POST;
            httpRequest.SetHttpContent(Encoding.UTF8.GetBytes("content"), null, FormatType.Xml);
            Assert.Equal("mgNkuembtIDdJeHwKEyFVQ==", DictionaryUtil.Get(httpRequest.Headers, "Content-MD5"));
        }
    }
}
