using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Http;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Policy;
using Aliyun.Credentials.Provider;
using Aliyun.Credentials.Utils;
using Moq;
using Xunit;

namespace aliyun_net_credentials_unit_tests.Provider
{
    public class EcsRamRoleCredentialProviderTest
    {
        [Fact]
        public async void TestCheckCredentialsUpdateAsynchronously()
        {
            var provider = new EcsRamRoleCredentialProvider.Builder().RoleName("roleName")
                .StaleValueBehavior(StaleValueBehavior.Strict)
                .AsyncCredentialUpdateEnabled(true)
                .JitterEnabled(true)
                .Build();
            Assert.Throws<CredentialException>(() => { provider.RefreshCredentials(); });
            await Task.Delay(1 * 60 * 1000 + 1000);
            provider.Dispose();
        }

        [Fact]
        public void TestGetPrefetchTime()
        {
            var expiration = 1735643602666L;
            var provider = new EcsRamRoleCredentialProvider.Builder().RoleName("roleName")
                .StaleValueBehavior(StaleValueBehavior.Strict)
                .AsyncCredentialUpdateEnabled(true)
                .JitterEnabled(true)
                .Build();
            var expect = DateTime.UtcNow.GetTimeMillis() + 60 * 60 * 1000;
            var prefetchTime = TestHelper.RunInstanceMethod(typeof(EcsRamRoleCredentialProvider),
                "GetPrefetchTime", provider, new object[] { expiration });

            Assert.True(expect <= (long)prefetchTime);
            Assert.True(expect + 10 >= (long)prefetchTime);

            expiration = 0;
            expect = DateTime.UtcNow.GetTimeMillis() + 5 * 60 * 1000;
            prefetchTime = TestHelper.RunInstanceMethod(typeof(EcsRamRoleCredentialProvider),
                "GetPrefetchTime", provider, new object[] { expiration });

            Assert.True(expect <= (long)prefetchTime);
            Assert.True(expect + 10 >= (long)prefetchTime);
        }

        [Fact]
        public void TestDisableIMDS()
        {
            var cacheMetadataDisabled = AuthUtils.EnvironmentEcsMetaDataDisabled;
            AuthUtils.EnvironmentEcsMetaDataDisabled = "true";
            var ex = Assert.Throws<CredentialException>(() => { new EcsRamRoleCredentialProvider.Builder().Build(); });
            Assert.Equal("IMDS credentials is disabled", ex.Message);

            AuthUtils.EnvironmentEcsMetaDataDisabled = null;
            var provider = new EcsRamRoleCredentialProvider.Builder().RoleName("roleName")
                .StaleValueBehavior(StaleValueBehavior.Strict)
                .AsyncCredentialUpdateEnabled(true)
                .JitterEnabled(true)
                .Build();

            AuthUtils.EnvironmentEcsMetaDataDisabled = "false";
            provider = new EcsRamRoleCredentialProvider.Builder().RoleName("roleName").Build();

            AuthUtils.EnvironmentEcsMetaDataDisabled = "test";
            provider = new EcsRamRoleCredentialProvider.Builder().RoleName("roleName").Build();

            Assert.True(provider.IsAsyncCredentialUpdateEnabled());

            AuthUtils.EnvironmentEcsMetaDataDisabled = cacheMetadataDisabled;
            provider.Dispose();
        }

        [Fact]
        public async void DisableIMDSv1Test()
        {
            var providerConfig =
                new EcsRamRoleCredentialProvider(new Config() { RoleName = "roleName", DisableIMDSv1 = true });
            Assert.True(providerConfig.DisableIMDSv1);

            providerConfig = new EcsRamRoleCredentialProvider.Builder().RoleName("roleName").DisableIMDSv1(true)
                .Build();
            Assert.True(providerConfig.DisableIMDSv1);

            var ex = Assert.Throws<CredentialException>(() => { providerConfig.GetCredentials(); });
            Assert.StartsWith(
                "Failed to get token from ECS Metadata Service, and fallback to IMDS v1 is disabled via the disableIMDSv1 configuration is turned on. Original error: Failed to connect ECS Metadata Service: ",
                ex.Message);
            ex = await Assert.ThrowsAsync<CredentialException>(async () =>
            {
                await providerConfig.GetCredentialsAsync();
            });
            Assert.StartsWith(
                "Failed to get token from ECS Metadata Service, and fallback to IMDS v1 is disabled via the disableIMDSv1 configuration is turned on. Original error: Failed to connect ECS Metadata Service: ",
                ex.Message);

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
                TestHelper.RunInstanceMethod(typeof(EcsRamRoleCredentialProvider), "GetMetadataToken",
                    providerConfig, new object[] { mock.Object });
            });
            Assert.Equal(
                "Failed to get token from ECS Metadata Service, and fallback to IMDS v1 is disabled via the disableIMDSv1 configuration is turned on. Original error: Failed to connect ECS Metadata Service: System.IO.IOException: test other exception",
                ex.Message);

            mock.Setup(p => p.DoActionAsync(It.IsAny<HttpRequest>()))
                .ThrowsAsync(new IOException("test other exception"));
            ex = Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethodAsync(typeof(EcsRamRoleCredentialProvider), "GetMetadataTokenAsync",
                    providerConfig, new object[] { mock.Object });
            });
            Assert.Equal(
                "Failed to get token from ECS Metadata Service, and fallback to IMDS v1 is disabled via the disableIMDSv1 configuration is turned on. Original error: Failed to connect ECS Metadata Service: System.IO.IOException: test other exception",
                ex.Message);

            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(response);
            ex = Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethod(typeof(EcsRamRoleCredentialProvider), "GetMetadataToken",
                    providerConfig, new object[] { mock.Object });
            });
            Assert.Equal(
                "Failed to get token from ECS Metadata Service, and fallback to IMDS v1 is disabled via the disableIMDSv1 configuration is turned on. Original error: Failed to get RAM session credentials from ECS metadata service. HttpCode=500, ResponseMessage=no token",
                ex.Message);

            mock.Setup(p => p.DoActionAsync(It.IsAny<HttpRequest>())).ReturnsAsync(response);
            ex = Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethodAsync(typeof(EcsRamRoleCredentialProvider), "GetMetadataTokenAsync",
                    providerConfig, new object[] { mock.Object });
            });
            Assert.Equal(
                "Failed to get token from ECS Metadata Service, and fallback to IMDS v1 is disabled via the disableIMDSv1 configuration is turned on. Original error: Failed to get RAM session credentials from ECS metadata service. HttpCode=500, ResponseMessage=no token",
                ex.Message);

            ex = Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethod(typeof(EcsRamRoleCredentialProvider), "CreateCredential",
                    providerConfig, new object[] { mock.Object });
            });
            Assert.Equal(
                "Failed to get token from ECS Metadata Service, and fallback to IMDS v1 is disabled via the disableIMDSv1 configuration is turned on. Original error: Failed to get RAM session credentials from ECS metadata service. HttpCode=500, ResponseMessage=no token",
                ex.Message);

            ex = Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethodAsync(typeof(EcsRamRoleCredentialProvider), "CreateCredentialAsync",
                    providerConfig, new object[] { mock.Object });
            });
            Assert.Equal(
                "Failed to get token from ECS Metadata Service, and fallback to IMDS v1 is disabled via the disableIMDSv1 configuration is turned on. Original error: Failed to get RAM session credentials from ECS metadata service. HttpCode=500, ResponseMessage=no token",
                ex.Message);

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
                TestHelper.RunInstanceMethodNew(typeof(EcsRamRoleCredentialProvider), "GetMetadata", providerConfig,
                    new object[] { mock.Object });
            });
            Assert.Equal("The role name was not found in the instance", ex.Message);

            ex = Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethodNew(typeof(EcsRamRoleCredentialProvider), "GetMetadata", providerConfig,
                    new object[] { mock.Object, "http://test" });
            });
            Assert.Equal("The role name was not found in the instance", ex.Message);
            providerConfig = new EcsRamRoleCredentialProvider.Builder().RoleName("roleName").Build();
            ex = Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethodNew(typeof(EcsRamRoleCredentialProvider), "GetMetadata", providerConfig,
                    new object[] { mock.Object });
            });
            Assert.Equal("The role name was not found in the instance", ex.Message);

            ex = Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethodNew(typeof(EcsRamRoleCredentialProvider), "GetMetadata", providerConfig,
                    new object[] { mock.Object, "http://test" });
            });
            Assert.Equal("The role name was not found in the instance", ex.Message);
            providerConfig.Dispose();
        }

        [Fact]
        public async void TestGetNewSessionCredentials()
        {
            var response = new HttpResponse("http://www.aliyun.com")
            {
                Status = 200, Encoding = "UTF-8", ContentType = FormatType.Json,
                Content = Encoding.UTF8.GetBytes(
                    "{\"Code\":\"Success\",  \"AccessKeyId\":\"test\", \"AccessKeySecret\":\"test\"}")
            };
            var mock = new Mock<IConnClient>();
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(response);
            var providerConfig = new EcsRamRoleCredentialProvider.Builder().Build();
            var ex = Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethod(typeof(EcsRamRoleCredentialProvider), "GetNewSessionCredentials",
                    providerConfig,
                    new object[] { mock.Object });
            });
            Assert.StartsWith("Error retrieving credentials from IMDS result: ", ex.Message);

            mock.Setup(p => p.DoActionAsync(It.IsAny<HttpRequest>())).ReturnsAsync(response);
            ex = Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethodAsync(typeof(EcsRamRoleCredentialProvider), "GetNewSessionCredentialsAsync",
                    providerConfig,
                    new object[] { mock.Object });
            });
            Assert.StartsWith("Error retrieving credentials from IMDS result: ", ex.Message);
            
            response = new HttpResponse("http://www.aliyun.com")
            {
                Status = 200, Encoding = "UTF-8", ContentType = FormatType.Json,
                Content = Encoding.UTF8.GetBytes(
                    "{\"Code\":\"Success\",  \"AccessKeyId\":\"test\", \"AccessKeySecret\":\"test\", \"SecurityToken\":\"test\"}")
            };
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(response);
            ex = Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethod(typeof(EcsRamRoleCredentialProvider), "GetNewSessionCredentials",
                    providerConfig,
                    new object[] { mock.Object });
            });
            Assert.Equal("Invalid json got from ECS Metadata service.", ex.Message);
            
            mock.Setup(p => p.DoActionAsync(It.IsAny<HttpRequest>())).ReturnsAsync(response);
            ex = Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethodAsync(typeof(EcsRamRoleCredentialProvider), "GetNewSessionCredentialsAsync",
                    providerConfig,
                    new object[] { mock.Object });
            });
            Assert.Equal("Invalid json got from ECS Metadata service.", ex.Message);
        }

        [Fact]
        public void EcsRamRoleProviderTest()
        {
            EcsRamRoleCredentialProvider providerRoleName = new EcsRamRoleCredentialProvider("roleName");
            Assert.NotNull(providerRoleName);
            Assert.Equal("roleName", providerRoleName.RoleName);
            Assert.Equal("ecs_ram_role", providerRoleName.GetProviderName());
            Assert.NotNull(providerRoleName.CredentialUrl);
            Assert.False(providerRoleName.DisableIMDSv1);

            EcsRamRoleCredentialProvider providerConfig =
                new EcsRamRoleCredentialProvider(new Config() { RoleName = "roleName" });
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
            Assert.StartsWith(
                "Failed to get token from ECS Metadata Service, and fallback to IMDS v1 is disabled via the disableIMDSv1 configuration is turned on. Original error: Failed to connect ECS Metadata Service: ",
                ex.Message);
            AuthUtils.DisableIMDSv1 = origin;
            providerConfig.Dispose();
        }

        [Fact]
        public async Task EcsRamRoleProviderAsyncTest()
        {
            EcsRamRoleCredentialProvider providerRoleName = new EcsRamRoleCredentialProvider("roleName");
            Assert.NotNull(providerRoleName);
            Assert.Equal("roleName", providerRoleName.RoleName);
            Assert.NotNull(providerRoleName.CredentialUrl);
            Assert.False(providerRoleName.DisableIMDSv1);

            providerRoleName = new EcsRamRoleCredentialProvider.Builder().RoleName("roleName").Build();
            Assert.NotNull(providerRoleName);
            Assert.Equal("roleName", providerRoleName.RoleName);
            Assert.NotNull(providerRoleName.CredentialUrl);
            Assert.False(providerRoleName.DisableIMDSv1);

            EcsRamRoleCredentialProvider providerConfig =
                new EcsRamRoleCredentialProvider(new Config() { RoleName = "roleName" });
            await Assert.ThrowsAsync<CredentialException>(async () =>
            {
                await providerConfig.RefreshCredentialsAsync();
            });

            providerConfig = new EcsRamRoleCredentialProvider.Builder().RoleName("roleName").Build();
            await Assert.ThrowsAsync<CredentialException>(async () =>
            {
                await providerConfig.RefreshCredentialsAsync();
            });
            providerConfig.Dispose();
        }

        [Fact]
        public void EcsRamRoleProviderClientTest()
        {
            EcsRamRoleCredentialProvider providerConfig = new EcsRamRoleCredentialProvider(
                new Config() { RoleName = "roleName" });
            Assert.Equal("roleName", providerConfig.RoleName);
            Assert.False(providerConfig.DisableIMDSv1);

            providerConfig = new EcsRamRoleCredentialProvider.Builder().RoleName("roleName").ConnectTimeout(1100)
                .ReadTimeout(1200).Build();
            Assert.Equal(1100, providerConfig.ConnectTimeout);
            Assert.Equal(1200, providerConfig.ReadTimeout);

            providerConfig = new EcsRamRoleCredentialProvider(
                new Config() { RoleName = "roleName", ConnectTimeout = 1100, Timeout = 1200 });
            Assert.False(providerConfig.DisableIMDSv1);
            Mock<IConnClient> mock = new Mock<IConnClient>();
            HttpResponse httpResponse = new HttpResponse("http://www.aliyun.com");
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(httpResponse);
            Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethod(typeof(EcsRamRoleCredentialProvider), "CreateCredential",
                    providerConfig, new object[] { mock.Object });
            });

            httpResponse = new HttpResponse("http://www.aliyun.com") { Status = 200 };
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(httpResponse);
            Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethod(typeof(EcsRamRoleCredentialProvider), "CreateCredential",
                    providerConfig, new object[] { mock.Object });
            });

            httpResponse = new HttpResponse("http://www.aliyun.com")
            {
                Status = 200, Encoding = "UTF-8", ContentType = FormatType.Json,
                Content = Encoding.UTF8.GetBytes(
                    "{\"Code\":\"fail\",  \"AccessKeyId\":\"test\", \"AccessKeySecret\":\"test\"}")
            };
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(httpResponse);
            var ex = Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethod(typeof(EcsRamRoleCredentialProvider), "CreateCredential",
                    providerConfig, new object[] { mock.Object });
            });

            httpResponse = new HttpResponse("http://www.aliyun.com")
            {
                Status = 200, Encoding = "UTF-8", ContentType = FormatType.Json,
                Content = Encoding.UTF8.GetBytes(
                    "{\"Code\":\"Success\",  \"AccessKeyId\":\"test\", \"AccessKeySecret\":\"test\", \"SecurityToken\":\"test\",  \"Expiration\":\"2019-08-08T01:01:01Z\"}")
            };
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(httpResponse);

            RefreshResult<CredentialModel> credential =
                (RefreshResult<CredentialModel>)TestHelper.RunInstanceMethod(typeof(EcsRamRoleCredentialProvider),
                    "CreateCredential", providerConfig, new object[] { mock.Object });
            Assert.NotNull(credential);
            Assert.Equal("test", credential.Value.AccessKeyId);

            httpResponse = new HttpResponse("http://www.aliyun.com")
            {
                Status = 200, Encoding = "UTF-8", ContentType = FormatType.Json,
                Content = Encoding.UTF8.GetBytes(
                    "{\"Code\":\"Fail\",  \"AccessKeyId\":\"test\", \"AccessKeySecret\":\"test\", \"SecurityToken\":\"test\",  \"Expiration\":\"2019-08-08T01:01:01Z\"}")
            };
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(httpResponse);
            ex = Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethod(typeof(EcsRamRoleCredentialProvider), "CreateCredential",
                    providerConfig, new object[] { mock.Object });
            });
            Assert.Equal("Failed to get RAM session credentials from ECS metadata service.", ex.Message);
            providerConfig.Dispose();
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
                TestHelper.RunInstanceMethodAsync(typeof(EcsRamRoleCredentialProvider), "CreateCredentialAsync",
                    providerConfig, new object[] { mock.Object });
            });

            httpResponse = new HttpResponse("http://www.aliyun.com") { Status = 200 };
            mock.Setup(p => p.DoActionAsync(It.IsAny<HttpRequest>())).ReturnsAsync(httpResponse);
            Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethodAsync(typeof(EcsRamRoleCredentialProvider), "CreateCredentialAsync",
                    providerConfig, new object[] { mock.Object });
            });

            httpResponse = new HttpResponse("http://www.aliyun.com")
            {
                Status = 200, Encoding = "UTF-8", ContentType = FormatType.Json,
                Content = Encoding.UTF8.GetBytes(
                    "{\"Code\":\"fail\",  \"AccessKeyId\":\"test\", \"AccessKeySecret\":\"test\"}")
            };
            mock.Setup(p => p.DoActionAsync(It.IsAny<HttpRequest>())).ReturnsAsync(httpResponse);
            Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethodAsync(typeof(EcsRamRoleCredentialProvider), "CreateCredentialAsync",
                    providerConfig, new object[] { mock.Object });
            });

            httpResponse = new HttpResponse("http://www.aliyun.com")
            {
                Status = 200, Encoding = "UTF-8", ContentType = FormatType.Json,
                Content = Encoding.UTF8.GetBytes(
                    "{\"Code\":\"Success\",  \"AccessKeyId\":\"test\", \"AccessKeySecret\":\"test\", \"SecurityToken\":\"test\",  \"Expiration\":\"2019-08-08T1:1:1Z\"}")
            };
            mock.Setup(p => p.DoActionAsync(It.IsAny<HttpRequest>())).ReturnsAsync(httpResponse);

            RefreshResult<CredentialModel> credentialModel =
                (RefreshResult<CredentialModel>)TestHelper.RunInstanceMethodAsync(typeof(EcsRamRoleCredentialProvider),
                    "CreateCredentialAsync", providerConfig, new object[] { mock.Object });
            Assert.NotNull(credentialModel);
            Assert.Equal("test", credentialModel.Value.AccessKeyId);
            Assert.Equal("ecs_ram_role", credentialModel.Value.ProviderName);
            providerConfig.Dispose();
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
                TestHelper.RunInstanceMethod(typeof(EcsRamRoleCredentialProvider), "GetRoleName", providerConfig,
                    new object[] { mock.Object });
            });

            httpResponse = new HttpResponse("http://www.aliyun.com") { Status = 200 };
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(httpResponse);
            Assert.Null(TestHelper.RunInstanceMethod(typeof(EcsRamRoleCredentialProvider), "GetRoleName",
                providerConfig, new object[] { mock.Object }));
            providerConfig.Dispose();
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
                TestHelper.RunInstanceMethodAsync(typeof(EcsRamRoleCredentialProvider), "GetRoleNameAsync",
                    providerConfig, new object[] { mock.Object });
            });

            httpResponse = new HttpResponse("http://www.aliyun.com") { Status = 200 };
            mock.Setup(p => p.DoActionAsync(It.IsAny<HttpRequest>())).ReturnsAsync(httpResponse);
            TestHelper.RunInstanceMethodAsync(typeof(EcsRamRoleCredentialProvider), "GetRoleNameAsync", providerConfig,
                new object[] { mock.Object });
            providerConfig.Dispose();
        }

        [Fact]
        public void TestGetCredentials()
        {
            EcsRamRoleCredentialProvider providerConfig = new EcsRamRoleCredentialProvider(
                new Config() { RoleName = "", ConnectTimeout = 1100, Timeout = 1200 });
            Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethod(typeof(EcsRamRoleCredentialProvider), "GetCredentials", providerConfig,
                    new object[] { });
            });

            providerConfig = new EcsRamRoleCredentialProvider(
                new Config() { RoleName = "roleName", ConnectTimeout = 1100, Timeout = 1200 });
            Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethod(typeof(EcsRamRoleCredentialProvider), "GetCredentials", providerConfig,
                    new object[] { });
            });
            providerConfig.Dispose();
        }

        [Fact]
        public void TestGetCredentialsAsync()
        {
            EcsRamRoleCredentialProvider providerConfig = new EcsRamRoleCredentialProvider(
                new Config() { RoleName = "", ConnectTimeout = 1100, Timeout = 1200 });
            Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethodAsync(typeof(EcsRamRoleCredentialProvider), "GetCredentialsAsync",
                    providerConfig, new object[] { });
            });

            providerConfig = new EcsRamRoleCredentialProvider(
                new Config() { RoleName = "roleName", ConnectTimeout = 1100, Timeout = 1200 });
            Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethodAsync(typeof(EcsRamRoleCredentialProvider), "GetCredentialsAsync",
                    providerConfig, new object[] { });
            });
            providerConfig.Dispose();
        }
    }
}