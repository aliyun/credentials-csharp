using System.Text;

using Aliyun.Credentials;
using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Http;
using Aliyun.Credentials.Provider;

using Moq;

using Xunit;

namespace aliyun_net_credentials_unit_tests.Provider
{
    public class RsaKeyPairCredentialProviderTest
    {
        [Fact]
        public void RsaKeyPairProviderTest()
        {
            Configuration config = new Configuration() { PublicKeyId = "publicKeyId", PrivateKeyFile = "privateKeyFile", ConnectTimeout = 20000, ReadTimeout = 15000 };
            RsaKeyPairCredentialProvider provider = new RsaKeyPairCredentialProvider(config);
            provider.DurationSeconds = 3650;
            provider.RegionId = "regionId";
            Assert.Equal("publicKeyId", provider.PublicKeyId);
            Assert.Equal("privateKeyFile", provider.PrivateKey);
            Assert.Equal(20000, provider.ConnectTimeout);
            Assert.Equal(15000, provider.ReadTimeout);
            Assert.Equal(3650, provider.DurationSeconds);
            Assert.Equal("regionId", provider.RegionId);

            provider.ConnectTimeout = 20001;
            provider.ReadTimeout = 15001;
            Assert.Equal(20001, provider.ConnectTimeout);
            Assert.Equal(15001, provider.ReadTimeout);

            Mock<IConnClient> mock = new Mock<IConnClient>();
            HttpResponse response = new HttpResponse("http://www.aliyun.com") { Status = 404 };
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(response);
            Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethod(typeof(RsaKeyPairCredentialProvider), "CreateCredential", provider, new object[] { mock.Object });
            });

            response = new HttpResponse("http://www.aliyun.com")
            {
                Status = 200,
                ContentType = FormatType.Json,
                Encoding = "UTF-8",
                Content = Encoding.UTF8.GetBytes("{\"SessionAccessKey1\":{\"Expiration\":\"2019-12-12T1:1:1Z\"}}")
            };
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(response);
            Assert.Throws<CredentialException>(() => { TestHelper.RunInstanceMethod(typeof(RsaKeyPairCredentialProvider), "CreateCredential", provider, new object[] { mock.Object }); });

            response = new HttpResponse("http://www.aliyun.com")
            {
                Status = 200,
                ContentType = FormatType.Json,
                Encoding = "UTF-8",
                Content = Encoding.UTF8.GetBytes("{\"SessionAccessKey\":{\"Expiration\":\"2019-12-12T1:1:1Z\",\"SessionAccessKeyId\":\"test\"," +
                "\"SessionAccessKeySecret\":\"test\"}}")
            };
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(response);
            Assert.IsType<RsaKeyPairCredential>(TestHelper.RunInstanceMethod(typeof(RsaKeyPairCredentialProvider), "CreateCredential", provider, new object[] { mock.Object }));
        }
    }
}
