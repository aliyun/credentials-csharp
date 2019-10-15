using System.Collections.Generic;
using System.IO;

using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Http;

using Xunit;

namespace aliyun_net_credentials_unit_tests.Http
{
    public class CompatibleUrlConnClientTest
    {
        [Fact]
        public void DoActionTest()
        {
            HttpRequest httpRequest = new HttpRequest("https://www.aliyun.com", new Dictionary<string, string>());
            httpRequest.Method = MethodType.Get;
            httpRequest.ConnectTimeout = 10000;
            httpRequest.ReadTimeout = 10000;
            CompatibleUrlConnClient client = new CompatibleUrlConnClient();
            HttpResponse httpResponse = client.DoAction(httpRequest);
            Assert.NotNull(httpResponse);

            httpRequest = new HttpRequest("http://www.aliyun.com");
            httpRequest.Method = MethodType.Get;
            httpRequest.ConnectTimeout = 1;
            httpRequest.ReadTimeout = 1;
            Assert.Throws<CredentialException>(() => client.DoAction(httpRequest));

            httpRequest = new HttpRequest();
            httpRequest.ConnectTimeout = 10000;
            httpRequest.ReadTimeout = 10000;
            Assert.Equal("URL is null for HttpRequest.",
                Assert.Throws<InvalidDataException>(() => client.DoAction(httpRequest)).Message
            );

            httpRequest = new HttpRequest("http://www.aliyun.com");
            httpRequest.ConnectTimeout = 10000;
            httpRequest.ReadTimeout = 10000;
            Assert.Equal("Method is null for HttpRequest.",
                Assert.Throws<InvalidDataException>(() => client.DoAction(httpRequest)).Message
            );

        }
    }
}
