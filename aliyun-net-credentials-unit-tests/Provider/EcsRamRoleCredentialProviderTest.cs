using System;
using System.IO;
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
    public class EcsRamRoleCredentialProviderTest
    {

        [Fact]
        public async void DisableIMDSv1Test()
        {
            EcsRamRoleCredentialProvider providerConfig = new EcsRamRoleCredentialProvider(new Config() { RoleName = "roleName", DisableIMDSv1 = true });
            Assert.True(providerConfig.DisableIMDSv1);

            var ex = Assert.Throws<CredentialException>(() => { providerConfig.GetCredentials(); });
            Assert.StartsWith("Failed to get token from ECS Metadata Service, and fallback to IMDS v1 is disabled via the disableIMDSv1 configuration is turned on. Original error: Failed to connect ECS Metadata Service: ", ex.Message);
            ex = await Assert.ThrowsAsync<CredentialException>(async () => { await providerConfig.GetCredentialsAsync(); });
            Assert.StartsWith("Failed to get token from ECS Metadata Service, and fallback to IMDS v1 is disabled via the disableIMDSv1 configuration is turned on. Original error: Failed to connect ECS Metadata Service: ", ex.Message);

            Mock<IConnClient> mock = new Mock<IConnClient>();
            HttpResponse response = new HttpResponse("http://test")
            {
                Status = 500,
                Encoding = "UTF-8",
                Content = Encoding.UTF8.GetBytes("no token")
            };
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Throws(new IOException("test other exception"));
            ex = Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethod(typeof(EcsRamRoleCredentialProvider), "GetMetadataToken", providerConfig, new object[] { mock.Object });
            });
            Assert.Equal("Failed to get token from ECS Metadata Service, and fallback to IMDS v1 is disabled via the disableIMDSv1 configuration is turned on. Original error: Failed to connect ECS Metadata Service: System.IO.IOException: test other exception", ex.Message);

            mock.Setup(p => p.DoActionAsync(It.IsAny<HttpRequest>())).ThrowsAsync(new IOException("test other exception"));
            ex = Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethodAsync(typeof(EcsRamRoleCredentialProvider), "GetMetadataTokenAsync", providerConfig, new object[] { mock.Object });
            });
            Assert.Equal("Failed to get token from ECS Metadata Service, and fallback to IMDS v1 is disabled via the disableIMDSv1 configuration is turned on. Original error: Failed to connect ECS Metadata Service: System.IO.IOException: test other exception", ex.Message);

            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(response);
            ex = Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethod(typeof(EcsRamRoleCredentialProvider), "GetMetadataToken", providerConfig, new object[] { mock.Object });
            });
            Assert.Equal("Failed to get token from ECS Metadata Service, and fallback to IMDS v1 is disabled via the disableIMDSv1 configuration is turned on. Original error: Failed to get RAM session credentials from ECS metadata service. HttpCode=500, ResponseMessage=no token", ex.Message);

            mock.Setup(p => p.DoActionAsync(It.IsAny<HttpRequest>())).ReturnsAsync(response);
            ex = Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethodAsync(typeof(EcsRamRoleCredentialProvider), "GetMetadataTokenAsync", providerConfig, new object[] { mock.Object });
            });
            Assert.Equal("Failed to get token from ECS Metadata Service, and fallback to IMDS v1 is disabled via the disableIMDSv1 configuration is turned on. Original error: Failed to get RAM session credentials from ECS metadata service. HttpCode=500, ResponseMessage=no token", ex.Message);

            ex = Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethod(typeof(EcsRamRoleCredentialProvider), "CreateCredential", providerConfig, new object[] { mock.Object });
            });
            Assert.Equal("Failed to get token from ECS Metadata Service, and fallback to IMDS v1 is disabled via the disableIMDSv1 configuration is turned on. Original error: Failed to get RAM session credentials from ECS metadata service. HttpCode=500, ResponseMessage=no token", ex.Message);

            ex = Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethodAsync(typeof(EcsRamRoleCredentialProvider), "CreateCredentialAsync", providerConfig, new object[] { mock.Object });
            });
            Assert.Equal("Failed to get token from ECS Metadata Service, and fallback to IMDS v1 is disabled via the disableIMDSv1 configuration is turned on. Original error: Failed to get RAM session credentials from ECS metadata service. HttpCode=500, ResponseMessage=no token", ex.Message);

            response = new HttpResponse("http://test")
            {
                Status = 404,
                Encoding = "UTF-8",
                Content = Encoding.UTF8.GetBytes("not found")
            };
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(response);
            providerConfig = new EcsRamRoleCredentialProvider("roleName");
            ex = Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethod(typeof(EcsRamRoleCredentialProvider), "GetMetadata", providerConfig, new object[] { mock.Object });
            });
            Assert.Equal("The role name was not found in the instance", ex.Message);
        }

        [Fact]
        public void EcsRamRoleProviderTest()
        {
            EcsRamRoleCredentialProvider providerRoleName = new EcsRamRoleCredentialProvider("roleName");
            Assert.NotNull(providerRoleName);
            Assert.Equal("roleName", providerRoleName.RoleName);
            Assert.NotNull(providerRoleName.CredentialUrl);
            Assert.False(providerRoleName.DisableIMDSv1);

            EcsRamRoleCredentialProvider providerConfig = new EcsRamRoleCredentialProvider(new Config() { RoleName = "roleName" });
            Assert.Throws<CredentialException>(() => { providerConfig.GetCredentials(); });
            bool origin = AuthUtils.DisableIMDSv1;
            AuthUtils.DisableIMDSv1 = true;
            Config config = new Config
            {
                RoleName = "roleName",
            };
            providerConfig = new EcsRamRoleCredentialProvider(config);
            Assert.True(providerConfig.DisableIMDSv1);
            var ex = Assert.Throws<CredentialException>(() => { providerConfig.GetCredentials(); });
            Assert.StartsWith("Failed to get token from ECS Metadata Service, and fallback to IMDS v1 is disabled via the disableIMDSv1 configuration is turned on. Original error: Failed to connect ECS Metadata Service: ", ex.Message);
            AuthUtils.DisableIMDSv1 = origin;
        }

        [Fact]
        public async Task EcsRamRoleProviderAsyncTest()
        {
            EcsRamRoleCredentialProvider providerRoleName = new EcsRamRoleCredentialProvider("roleName");
            Assert.NotNull(providerRoleName);
            Assert.Equal("roleName", providerRoleName.RoleName);
            Assert.NotNull(providerRoleName.CredentialUrl);
            Assert.False(providerRoleName.DisableIMDSv1);

            EcsRamRoleCredentialProvider providerConfig = new EcsRamRoleCredentialProvider(new Config() { RoleName = "roleName" });
            await Assert.ThrowsAsync<CredentialException>(async () => { await providerConfig.RefreshCredentialsAsync(); });
        }

        [Fact]
        public void EcsRamRoleProviderClientTest()
        {
            EcsRamRoleCredentialProvider providerConfig = new EcsRamRoleCredentialProvider(
                new Config() { RoleName = "roleName" });
            Assert.Equal("roleName", providerConfig.RoleName);
            Assert.False(providerConfig.DisableIMDSv1);

            providerConfig = new EcsRamRoleCredentialProvider(
                new Config() { RoleName = "roleName", ConnectTimeout = 1100, Timeout = 1200 });
            Assert.False(providerConfig.DisableIMDSv1);
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

            httpResponse = new HttpResponse("http://www.aliyun.com") { Status = 200, Encoding = "UTF-8", ContentType = FormatType.Json, Content = Encoding.UTF8.GetBytes("{\"Code\":\"Success\",  \"AccessKeyId\":\"test\", \"AccessKeySecret\":\"test\", \"SecurityToken\":\"test\",  \"Expiration\":\"2019-08-08T01:01:01Z\"}") };
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(httpResponse);

            RefreshResult<CredentialModel> credential = (RefreshResult<CredentialModel>)TestHelper.RunInstanceMethod(typeof(EcsRamRoleCredentialProvider), "CreateCredential", providerConfig, new object[] { mock.Object });
            Assert.NotNull(credential);
            Assert.Equal("test", credential.Value.AccessKeyId);

            httpResponse = new HttpResponse("http://www.aliyun.com") { Status = 200, Encoding = "UTF-8", ContentType = FormatType.Json, Content = Encoding.UTF8.GetBytes("{\"Code\":\"Fail\",  \"AccessKeyId\":\"test\", \"AccessKeySecret\":\"test\", \"SecurityToken\":\"test\",  \"Expiration\":\"2019-08-08T01:01:01Z\"}") };
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(httpResponse);
            var ex = Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethod(typeof(EcsRamRoleCredentialProvider), "CreateCredential", providerConfig, new object[] { mock.Object });
            });
            Assert.Equal("Failed to get RAM session credentials from ECS metadata service.", ex.Message);
        }

        [Fact]
        public void EcsRamRoleProviderClientAsyncTest()
        {
            EcsRamRoleCredentialProvider providerConfig = new EcsRamRoleCredentialProvider(
                new Config() { RoleName = "roleName" });
            Assert.Equal("roleName", providerConfig.RoleName);

            providerConfig = new EcsRamRoleCredentialProvider(
                new Config() { RoleName = "roleName", ConnectTimeout = 1100, Timeout = 1200 });
            Mock<IConnClient> mock = new Mock<IConnClient>();
            HttpResponse httpResponse = new HttpResponse("http://www.aliyun.com");
            mock.Setup(p => p.DoActionAsync(It.IsAny<HttpRequest>())).ReturnsAsync(httpResponse);
            Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethodAsync(typeof(EcsRamRoleCredentialProvider), "CreateCredentialAsync", providerConfig, new object[] { mock.Object });
            });

            httpResponse = new HttpResponse("http://www.aliyun.com") { Status = 200 };
            mock.Setup(p => p.DoActionAsync(It.IsAny<HttpRequest>())).ReturnsAsync(httpResponse);
            Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethodAsync(typeof(EcsRamRoleCredentialProvider), "CreateCredentialAsync", providerConfig, new object[] { mock.Object });
            });

            httpResponse = new HttpResponse("http://www.aliyun.com") { Status = 200, Encoding = "UTF-8", ContentType = FormatType.Json, Content = Encoding.UTF8.GetBytes("{\"Code\":\"fail\",  \"AccessKeyId\":\"test\", \"AccessKeySecret\":\"test\"}") };
            mock.Setup(p => p.DoActionAsync(It.IsAny<HttpRequest>())).ReturnsAsync(httpResponse);
            Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethodAsync(typeof(EcsRamRoleCredentialProvider), "CreateCredentialAsync", providerConfig, new object[] { mock.Object });
            });

            httpResponse = new HttpResponse("http://www.aliyun.com") { Status = 200, Encoding = "UTF-8", ContentType = FormatType.Json, Content = Encoding.UTF8.GetBytes("{\"Code\":\"Success\",  \"AccessKeyId\":\"test\", \"AccessKeySecret\":\"test\", \"SecurityToken\":\"test\",  \"Expiration\":\"2019-08-08T1:1:1Z\"}") };
            mock.Setup(p => p.DoActionAsync(It.IsAny<HttpRequest>())).ReturnsAsync(httpResponse);

            RefreshResult<CredentialModel> credentialModel = (RefreshResult<CredentialModel>)TestHelper.RunInstanceMethodAsync(typeof(EcsRamRoleCredentialProvider), "CreateCredentialAsync", providerConfig, new object[] { mock.Object });
            Assert.NotNull(credentialModel);
            Assert.Equal("test", credentialModel.Value.AccessKeyId);
        }

        [Fact]
        public void TestGetRoleName()
        {
            EcsRamRoleCredentialProvider providerConfig = new EcsRamRoleCredentialProvider(
                new Config() { RoleName = "", ConnectTimeout = 1100, Timeout = 1200 });
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
        public void TestGetRoleNameAsync()
        {
            EcsRamRoleCredentialProvider providerConfig = new EcsRamRoleCredentialProvider(
                new Config() { RoleName = "", ConnectTimeout = 1100, Timeout = 1200 });
            Mock<IConnClient> mock = new Mock<IConnClient>();
            HttpResponse httpResponse = new HttpResponse("http://www.aliyun.com");
            mock.Setup(p => p.DoActionAsync(It.IsAny<HttpRequest>())).ReturnsAsync(httpResponse);
            Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethodAsync(typeof(EcsRamRoleCredentialProvider), "GetRoleNameAsync", providerConfig, new object[] { mock.Object });
            });

            httpResponse = new HttpResponse("http://www.aliyun.com") { Status = 200 };
            mock.Setup(p => p.DoActionAsync(It.IsAny<HttpRequest>())).ReturnsAsync(httpResponse);
            TestHelper.RunInstanceMethodAsync(typeof(EcsRamRoleCredentialProvider), "GetRoleNameAsync", providerConfig, new object[] { mock.Object });
        }

        [Fact]
        public void TestGetCredentials()
        {
            EcsRamRoleCredentialProvider providerConfig = new EcsRamRoleCredentialProvider(
                new Config() { RoleName = "", ConnectTimeout = 1100, Timeout = 1200 });
            Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethod(typeof(EcsRamRoleCredentialProvider), "GetCredentials", providerConfig, new object[] { });
            });

            providerConfig = new EcsRamRoleCredentialProvider(
                new Config() { RoleName = "roleName", ConnectTimeout = 1100, Timeout = 1200 });
            Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethod(typeof(EcsRamRoleCredentialProvider), "GetCredentials", providerConfig, new object[] { });
            });
        }

        [Fact]
        public void TestGetCredentialsAsync()
        {
            EcsRamRoleCredentialProvider providerConfig = new EcsRamRoleCredentialProvider(
                new Config() { RoleName = "", ConnectTimeout = 1100, Timeout = 1200 });
            Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethodAsync(typeof(EcsRamRoleCredentialProvider), "GetCredentialsAsync", providerConfig, new object[] { });
            });

            providerConfig = new EcsRamRoleCredentialProvider(
                new Config() { RoleName = "roleName", ConnectTimeout = 1100, Timeout = 1200 });
            Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethodAsync(typeof(EcsRamRoleCredentialProvider), "GetCredentialsAsync", providerConfig, new object[] { });
            });
        }
    }
}
