using System;
using System.Text;

using Aliyun.Credentials;
using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Http;
using Aliyun.Credentials.Provider;

using Moq;

using Xunit;

namespace aliyun_net_credentials_unit_tests.Provider
{
    public class EcsRamRoleCredentialProviderTest
    {
        [Fact]
        public void EcsRamRoleProviderTest()
        {
            EcsRamRoleCredentialProvider providerRoleName = new EcsRamRoleCredentialProvider("roleName");
            Assert.NotNull(providerRoleName);
            Assert.Equal("roleName", providerRoleName.RoleName);
            Assert.NotNull(providerRoleName.CredentialUrl);

            EcsRamRoleCredentialProvider providerConfig = new EcsRamRoleCredentialProvider(new Configuration() { RoleName = "roleName" });
            Assert.Throws<CredentialException>(() => { providerConfig.GetCredentials(); });

        }

        [Fact]
        public void EcsRamRoleProviderClientTest()
        {
            EcsRamRoleCredentialProvider providerConfig = new EcsRamRoleCredentialProvider(
                new Configuration() { RoleName = "roleName" });
            Assert.Equal("roleName", providerConfig.RoleName);

            providerConfig = new EcsRamRoleCredentialProvider(
                new Configuration() { RoleName = "roleName", ConnectTimeout = 1100, ReadTimeout = 1200 });
            Mock<IConnClient> mock = new Mock<IConnClient>();
            HttpResponse httpResponse = new HttpResponse("http://www.aliyun.com");
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(httpResponse);
            Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethod(typeof(EcsRamRoleCredentialProvider), "CreateCredential", providerConfig, new object[] { mock.Object });
            });

            httpResponse = new HttpResponse("http://www.aliyun.com") { Status = 200 };
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(httpResponse);
            Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethod(typeof(EcsRamRoleCredentialProvider), "CreateCredential", providerConfig, new object[] { mock.Object });
            });

            httpResponse = new HttpResponse("http://www.aliyun.com") { Status = 200, Encoding = "UTF-8", ContentType = FormatType.Json, Content = Encoding.UTF8.GetBytes("{\"Code\":\"fail\",  \"AccessKeyId\":\"test\", \"AccessKeySecret\":\"test\"}") };
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(httpResponse);
            Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethod(typeof(EcsRamRoleCredentialProvider), "CreateCredential", providerConfig, new object[] { mock.Object });
            });

            httpResponse = new HttpResponse("http://www.aliyun.com") { Status = 200, Encoding = "UTF-8", ContentType = FormatType.Json, Content = Encoding.UTF8.GetBytes("{\"Code\":\"Success\",  \"AccessKeyId\":\"test\", \"AccessKeySecret\":\"test\", \"SecurityToken\":\"test\",  \"Expiration\":\"2019-08-08T1:1:1Z\"}") };
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(httpResponse);

            EcsRamRoleCredential credential = (EcsRamRoleCredential) TestHelper.RunInstanceMethod(typeof(EcsRamRoleCredentialProvider), "CreateCredential", providerConfig, new object[] { mock.Object });
            Assert.NotNull(credential);
        }

        [Fact]
        public void TestGetRoleName()
        {
            EcsRamRoleCredentialProvider providerConfig = new EcsRamRoleCredentialProvider(
                new Configuration() { RoleName = "", ConnectTimeout = 1100, ReadTimeout = 1200 });
            Mock<IConnClient> mock = new Mock<IConnClient>();
            HttpResponse httpResponse = new HttpResponse("http://www.aliyun.com");
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(httpResponse);
            Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethod(typeof(EcsRamRoleCredentialProvider), "GetRoleName", providerConfig, new object[] { mock.Object });
            });

            httpResponse = new HttpResponse("http://www.aliyun.com") { Status = 200 };
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(httpResponse);
            Assert.Null(TestHelper.RunInstanceMethod(typeof(EcsRamRoleCredentialProvider), "GetRoleName", providerConfig, new object[] { mock.Object }));
        }

        [Fact]
        public void TestGetCredentials()
        {
            EcsRamRoleCredentialProvider providerConfig = new EcsRamRoleCredentialProvider(
                new Configuration() { RoleName = "", ConnectTimeout = 1100, ReadTimeout = 1200 });
            Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethod(typeof(EcsRamRoleCredentialProvider), "GetCredentials", providerConfig, new object[] { });
            });

            providerConfig = new EcsRamRoleCredentialProvider(
                new Configuration() { RoleName = "roleName", ConnectTimeout = 1100, ReadTimeout = 1200 });
            Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethod(typeof(EcsRamRoleCredentialProvider), "GetCredentials", providerConfig, new object[] { });
            });
        }
    }
}
