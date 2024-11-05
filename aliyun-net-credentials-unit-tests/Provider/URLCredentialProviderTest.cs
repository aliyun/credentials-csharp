using System;
using System.Text;
using System.Threading.Tasks;

using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Http;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Provider;
using Aliyun.Credentials.Utils;

using Moq;
using Xunit;

namespace aliyun_net_credentials_unit_tests.Provider
{
    public class URLCredentialProviderTest
    {
        [Fact]
        public void TestConstructor()
        {
            URLCredentialProvider provider;
            var ex = Assert.Throws<ArgumentNullException>(() => provider = new URLCredentialProvider(""));
            Assert.StartsWith("Credentials URI must not be null or empty.", ex.Message);
            ex = Assert.Throws<ArgumentNullException>(() => provider = new URLCredentialProvider.Builder().CredentialsURI("").Build());
            Assert.StartsWith("Credentials URI must not be null or empty.", ex.Message);
            var ex1 = Assert.Throws<CredentialException>(() => provider = new URLCredentialProvider("url"));
            Assert.StartsWith("Credential URI is not valid.", ex1.Message);
            ex1 = Assert.Throws<CredentialException>(() => provider = new URLCredentialProvider.Builder().CredentialsURI("url").Build());
            Assert.StartsWith("Credential URI is not valid.", ex1.Message);
            provider = new URLCredentialProvider("http://test");
            provider = new URLCredentialProvider.Builder().CredentialsURI("http://test").Build();
            provider = new URLCredentialProvider.Builder().CredentialsURI(new Uri("http://test")).Build();
        }

        [Fact]
        public async void TestGetCredentials()
        {
            Config config = new Config
            {
                CredentialsURI = "http://10.10.10.10",
                ConnectTimeout = 2000,
                Timeout = 2000,
            };

            URLCredentialProvider provider = new URLCredentialProvider(config);
            var ex = Assert.Throws<CredentialException>(() => { provider.GetCredentials(); });
            Assert.StartsWith("Failed to connect Server: http://10.10.10.10", ex.Message);

            provider = new URLCredentialProvider.Builder()
                .CredentialsURI("http://10.10.10.10")
                .ConnectTimeout(2000)
                .ReadTimeout(2000)
                .Build();

            ex = Assert.Throws<CredentialException>(() => { provider.GetCredentials(); });
            Assert.StartsWith("Failed to connect Server: http://10.10.10.10", ex.Message);

            ex = await Assert.ThrowsAsync<CredentialException>(async () => { await provider.GetCredentialsAsync(); });
            Assert.StartsWith("Failed to connect Server: http://10.10.10.10", ex.Message);

            var supplier = new RefreshCachedSupplier<CredentialModel>(new Func<RefreshResult<CredentialModel>>(provider.RefreshCredentials), new Func<Task<RefreshResult<CredentialModel>>>(provider.RefreshCredentialsAsync));
            ex = Assert.Throws<CredentialException>(() => { supplier.Get(); });
        }

        [Fact]
        public async Task TestCreateCredentialAsync()
        {
            Config config = new Config
            {
                CredentialsURI = "http://10.10.10.10",
            };
            URLCredentialProvider provider = new URLCredentialProvider(config);

            Mock<IConnClient> mock = new Mock<IConnClient>();
            HttpResponse response = new HttpResponse("http://www.aliyun.com")
            {
                Status = 200,
                Encoding = "UTF-8",
                Content = Encoding.UTF8.GetBytes("{\"Expiration\":\"2019-01-01T1:1:1Z\",\"Code\":\"Success\",\"AccessKeyId\":\"test\"," +
                "\"AccessKeySecret\":\"test\",\"SecurityToken\":\"test\"}")
            };
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(response);
            Assert.IsType<RefreshResult<CredentialModel>>(TestHelper.RunInstanceMethod(typeof(URLCredentialProvider), "CreateCredential", provider, new object[] { mock.Object }));
            RefreshResult<CredentialModel> mockRefreshResult = (RefreshResult<CredentialModel>)TestHelper.RunInstanceMethod(typeof(URLCredentialProvider), "CreateCredential", provider, new object[] { mock.Object });
            Assert.Equal(AuthConstant.URLSts, mockRefreshResult.Value.Type);
            Assert.Equal(AuthConstant.CredentialsURI, mockRefreshResult.Value.Type);
            Assert.Equal("credentials_uri", mockRefreshResult.Value.ProviderName);

            mock.Setup(p => p.DoActionAsync(It.IsAny<HttpRequest>())).ReturnsAsync(response);
            Assert.IsType<RefreshResult<CredentialModel>>(TestHelper.RunInstanceMethodAsync(typeof(URLCredentialProvider), "CreateCredentialAsync", provider, new object[] { mock.Object }));
            RefreshResult<CredentialModel> mockRefreshResultAsync = (RefreshResult<CredentialModel>)TestHelper.RunInstanceMethodAsync(typeof(URLCredentialProvider), "CreateCredentialAsync", provider, new object[] { mock.Object });
            Assert.Equal(AuthConstant.URLSts, mockRefreshResultAsync.Value.Type);
            Assert.Equal(AuthConstant.CredentialsURI, mockRefreshResult.Value.Type);

            response.Status = 400;
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(response);
            var ex = Assert.Throws<CredentialException>(() => { TestHelper.RunInstanceMethod(typeof(URLCredentialProvider), "CreateCredential", provider, new object[] { mock.Object }); });
            Assert.Equal("Failed to get credentials from server: http://10.10.10.10/\nHttpCode=400\nHttpRAWContent={\"Expiration\":\"2019-01-01T1:1:1Z\",\"Code\":\"Success\",\"AccessKeyId\":\"test\",\"AccessKeySecret\":\"test\",\"SecurityToken\":\"test\"}", ex.Message);

            response.Status = 100;
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(response);
            ex = Assert.Throws<CredentialException>(() => { TestHelper.RunInstanceMethodAsync(typeof(URLCredentialProvider), "CreateCredentialAsync", provider, new object[] { mock.Object }); });
            Assert.Equal("Failed to get credentials from server: http://10.10.10.10/\nHttpCode=100\nHttpRAWContent={\"Expiration\":\"2019-01-01T1:1:1Z\",\"Code\":\"Success\",\"AccessKeyId\":\"test\",\"AccessKeySecret\":\"test\",\"SecurityToken\":\"test\"}", ex.Message);

            response.Content = Encoding.UTF8.GetBytes("{\"Expiration\":\"2019-01-01T1:1:1Z\",\"Code\":\"Fail\",\"AccessKeyId\":\"test\"," +
                "\"AccessKeySecret\":\"test\",\"SecurityToken\":\"test\"}");
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(response);
            ex = Assert.Throws<CredentialException>(() => { TestHelper.RunInstanceMethod(typeof(URLCredentialProvider), "CreateCredential", provider, new object[] { mock.Object }); });
            Assert.Equal("Failed to get credentials from server: http://10.10.10.10/\nHttpCode=100\nHttpRAWContent={\"Expiration\":\"2019-01-01T1:1:1Z\",\"Code\":\"Fail\",\"AccessKeyId\":\"test\",\"AccessKeySecret\":\"test\",\"SecurityToken\":\"test\"}", ex.Message);

            response.Content = Encoding.UTF8.GetBytes("{\"Expiration\":\"2019-01-01T1:1:1Z\",\"AccessKeyId\":\"test\"," +
                            "\"AccessKeySecret\":\"test\",\"SecurityToken\":\"test\"}");
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(response);
            ex = Assert.Throws<CredentialException>(() => { TestHelper.RunInstanceMethodAsync(typeof(URLCredentialProvider), "CreateCredentialAsync", provider, new object[] { mock.Object }); });
            Assert.Equal("Failed to get credentials from server: http://10.10.10.10/\nHttpCode=100\nHttpRAWContent={\"Expiration\":\"2019-01-01T1:1:1Z\",\"AccessKeyId\":\"test\",\"AccessKeySecret\":\"test\",\"SecurityToken\":\"test\"}", ex.Message);
        }
    }
}