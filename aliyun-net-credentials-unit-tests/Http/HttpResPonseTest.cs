using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Aliyun.Credentials.Http;

using Moq;

using Xunit;

namespace aliyun_net_credentials_unit_tests.Http
{
    public class HttpResponseTest
    {
        private readonly string requestUrl = "https://www.aliyun.com/";

        [Fact]
        public void GetResponse()
        {
            var request = new HttpResponse(requestUrl);
            var content = Encoding.ASCII.GetBytes("someString");
            request.SetContent(content, "UTF-8", FormatType.Form);
            request.Method = MethodType.Get;
            var response = HttpResponse.GetResponse(request);
            Assert.Equal("UTF-8", response.Encoding);
            Assert.Equal(MethodType.Get, response.Method);
            HttpResponse.GetResponse(request, 30000);
        }

        [Fact]
        public async Task GetResponseAsync()
        {
            var request = new HttpResponse(requestUrl);
            var content = Encoding.ASCII.GetBytes("someString");
            request.SetContent(content, "UTF-8", FormatType.Form);
            request.Method = MethodType.Get;
            var response = await HttpResponse.GetResponseAsync(request);
            Assert.Equal("UTF-8", response.Encoding);
            Assert.Equal(MethodType.Get, response.Method);

            response = await HttpResponse.GetResponseAsync(request, 30000);
            Assert.NotNull(response);
        }

        [Fact]
        public void GetWebRequest()
        {
            var request = HttpResponseTest.SetContent();
            var httpWebRequest = HttpResponse.GetWebRequest(request);
            Assert.IsType<HttpWebRequest>(httpWebRequest);
            Assert.Equal("text/json", httpWebRequest.ContentType);

            request.Headers.Add("Accept", "accept");
            request.Headers.Add("Date", "Thu, 24 Jan 2019 05:16:46 GMT");

            request.Method = MethodType.Post;
            httpWebRequest = HttpResponse.GetWebRequest(request);
            Assert.IsType<HttpWebRequest>(httpWebRequest);
            Assert.Equal("text/json", httpWebRequest.ContentType);
        }

        [Fact]
        public async Task GetWebRequestAsync()
        {
            var request = HttpResponseTest.SetContent();
            var httpWebRequest = await HttpResponse.GetWebRequestAsync(request);
            Assert.IsType<HttpWebRequest>(httpWebRequest);
            Assert.Equal("text/json", httpWebRequest.ContentType);

            request.Headers.Add("Accept", "accept");
            request.Headers.Add("Date", "Thu, 24 Jan 2019 05:16:46 GMT");

            request.Method = MethodType.Post;
            httpWebRequest = await HttpResponse.GetWebRequestAsync(request);
            Assert.IsType<HttpWebRequest>(httpWebRequest);
            Assert.Equal("text/json", httpWebRequest.ContentType);
        }

        [Fact]
        public static HttpRequest SetContent()
        {
            var tmpHeaders = new Dictionary<string, string>
                { { "Content-MD5", "md5" },
                    { "Content-Length", "1024" },
                    { "Content-Type", "text/json" }
                };
            var instance = new HttpRequest("https://www.alibabacloud.com", tmpHeaders);
            instance.Method = MethodType.Get;
            Assert.Equal(MethodType.Get, instance.Method);

            // when content is null
            instance.Content = null;
            instance.Encoding = "UTF-8";
            instance.ContentType = FormatType.Json;
            Assert.Null(instance.Content);

            // When content is not null
            var content = Encoding.ASCII.GetBytes("someString");
            instance.Content = content;
            Assert.NotNull(instance.Content);
            Assert.Equal(content, instance.Content);

            // when formatType is null
            instance.Content = content;
            instance.ContentType = null;
            Assert.NotNull(instance.Content);
            Assert.Equal(content, instance.Content);
            Assert.Equal(FormatType.Json, instance.ContentType);

            return instance;
        }

        [Fact]
        public void ParseHttpResponseTest()
        {
            HttpWebResponse httpWebResponse = new HttpWebResponse();
            Assert.NotNull(httpWebResponse);

            HttpResponse httpResponse = new HttpResponse("http://www.baidu.com");

            Mock<HttpWebResponse> mock = new Mock<HttpWebResponse>();
            mock.Setup(p => p.GetResponseStream()).Returns((MemoryStream) null);
            mock.Setup(p => p.Method).Returns("Get");
            WebHeaderCollection headers = new WebHeaderCollection();
            headers.Add("test", "test");
            mock.Setup(p => p.Headers).Returns(headers);
            TestHelper.RunStaticMethod(typeof(HttpResponse), "ParseHttpResponse", new object[] { httpResponse, mock.Object });
            byte[] bytes = Encoding.UTF8.GetBytes("test");
            mock.Setup(p => p.GetResponseStream()).Returns(new MemoryStream(bytes));

            TestHelper.RunStaticMethod(typeof(HttpResponse), "ParseHttpResponse", new object[] { httpResponse, mock.Object });
            Assert.Null(httpResponse.Encoding);

            headers.Add("Content-Type", "");
            mock.Setup(p => p.GetResponseStream()).Returns(new MemoryStream(bytes));
            mock.Setup(p => p.Headers).Returns(headers);
            TestHelper.RunStaticMethod(typeof(HttpResponse), "ParseHttpResponse", new object[] { httpResponse, mock.Object });

            headers.Add("Content-Type", "test");
            mock.Setup(p => p.GetResponseStream()).Returns(new MemoryStream(bytes));
            mock.Setup(p => p.Headers).Returns(headers);
            TestHelper.RunStaticMethod(typeof(HttpResponse), "ParseHttpResponse", new object[] { httpResponse, mock.Object });
        }

        [Fact]
        public async Task ParseHttpResponseAsyncTest()
        {
            HttpWebResponse httpWebResponse = new HttpWebResponse();
            Assert.NotNull(httpWebResponse);

            HttpResponse httpResponse = new HttpResponse("http://www.baidu.com");

            Mock<HttpWebResponse> mock = new Mock<HttpWebResponse>();
            mock.Setup(p => p.GetResponseStream()).Returns((MemoryStream) null);
            mock.Setup(p => p.Method).Returns("Get");
            WebHeaderCollection headers = new WebHeaderCollection();
            headers.Add("test", "test");
            mock.Setup(p => p.Headers).Returns(headers);
            TestHelper.RunStaticMethod(typeof(HttpResponse), "ParseHttpResponseAsync", new object[] { httpResponse, mock.Object });
            byte[] bytes = Encoding.UTF8.GetBytes("test");
            mock.Setup(p => p.GetResponseStream()).Returns(new MemoryStream(bytes));

            TestHelper.RunStaticMethod(typeof(HttpResponse), "ParseHttpResponseAsync", new object[] { httpResponse, mock.Object });
            Assert.Null(httpResponse.Encoding);

            headers.Add("Content-Type", "");
            mock.Setup(p => p.GetResponseStream()).Returns(new MemoryStream(bytes));
            mock.Setup(p => p.Headers).Returns(headers);
            TestHelper.RunStaticMethod(typeof(HttpResponse), "ParseHttpResponseAsync", new object[] { httpResponse, mock.Object });

            headers.Add("Content-Type", "test");
            mock.Setup(p => p.GetResponseStream()).Returns(new MemoryStream(bytes));
            mock.Setup(p => p.Headers).Returns(headers);
            TestHelper.RunStaticMethod(typeof(HttpResponse), "ParseHttpResponseAsync", new object[] { httpResponse, mock.Object });
        }

        [Fact]
        public void HttpCheckTest()
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.Url = null;
            Assert.Equal("URL is null for HttpRequest.",
                Assert.Throws<InvalidDataException>(() =>
                {
                    TestHelper.RunStaticMethod(typeof(HttpResponse), "CheckHttpRequest", new object[] { httpRequest });
                }).Message
            );

            httpRequest.Url = "http://www.aliyun.com";
            Assert.Equal("Method is null for HttpRequest.",
                Assert.Throws<InvalidDataException>(() =>
                {
                    TestHelper.RunStaticMethod(typeof(HttpResponse), "CheckHttpRequest", new object[] { httpRequest });
                }).Message
            );
        }
    }
}
