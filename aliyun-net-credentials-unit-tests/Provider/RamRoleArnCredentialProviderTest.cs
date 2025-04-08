using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aliyun.Credentials;
using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Http;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Provider;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace aliyun_net_credentials_unit_tests.Provider
{
    public class RamRoleArnCredentialProviderTest
    {
        [Fact]
        public void TestProviderConstructor()
        {
            SessionCredentialsProvider nullProvider = null;
            RamRoleArnCredentialProvider provider;
            var ex = Assert.Throws<ArgumentNullException>(() =>
                provider = new RamRoleArnCredentialProvider(nullProvider, "roleArn"));
            Assert.StartsWith("Must specify a previous credentials provider to asssume role.", ex.Message);
            ex = Assert.Throws<ArgumentNullException>(() =>
                provider = new RamRoleArnCredentialProvider.Builder().CredentialsProvider(nullProvider)
                    .RoleArn("roleArn").Build());
            Assert.StartsWith("AccessKeyId must not be null or empty.", ex.Message);
            provider = new RamRoleArnCredentialProvider.Builder()
                .CredentialsProvider(nullProvider)
                .RoleArn("roleArn")
                .AccessKeyId("test")
                .AccessKeySecret("test")
                .STSEndpoint("sts.cn-hangzhou.aliyuncs.com")
                .Build();
            Assert.Equal("sts.cn-hangzhou.aliyuncs.com", provider.GetSTSEndpoint());
            Assert.Equal("StaticAKCredentialsProvider", provider.CredentialsProvider.GetType().Name);

            provider = new RamRoleArnCredentialProvider.Builder()
                .CredentialsProvider(nullProvider)
                .RoleArn("roleArn")
                .AccessKeyId("test")
                .AccessKeySecret("test")
                .SecurityToken("test")
                .ExternalId("test")
                .Build();
            Assert.Equal("sts.aliyuncs.com", provider.GetSTSEndpoint());
            Assert.Equal("test", provider.GetExternalId());
            Assert.Equal("StaticSTSCredentialsProvider", provider.CredentialsProvider.GetType().Name);
        }

#if NET45
        internal static TValue GetValueOrDefault<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key,
            TValue defaultValue = default)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : defaultValue;
        }
#endif

        [Fact]
        public async void TestRealRequest()
        {
            Config config = new Config()
                { AccessKeyId = "accessKeyId", AccessKeySecret = "accessKeySecret", RoleArn = "roleArn" };
            RamRoleArnCredentialProvider provider = new RamRoleArnCredentialProvider(config);
            var supplier = new RefreshCachedSupplier<CredentialModel>(
                new Func<RefreshResult<CredentialModel>>(provider.RefreshCredentials),
                new Func<Task<RefreshResult<CredentialModel>>>(provider.RefreshCredentialsAsync));
            var ex = Assert.Throws<CredentialException>(() => { supplier.Get(); });
            Dictionary<string, object> msgMap = JsonConvert.DeserializeObject<Dictionary<string, object>>(ex.Message);
#if NET45
            Assert.NotNull(GetValueOrDefault(msgMap,"RequestId"));
            Assert.Equal("Specified access key is not found.", GetValueOrDefault(msgMap,"Message"));
            Assert.Equal("sts.aliyuncs.com", GetValueOrDefault(msgMap,"HostId"));
            Assert.Equal("InvalidAccessKeyId.NotFound", GetValueOrDefault(msgMap,"Code"));
#else
            Assert.NotNull(msgMap.GetValueOrDefault("RequestId"));
            Assert.Equal("Specified access key is not found.", msgMap.GetValueOrDefault("Message"));
            Assert.Equal("sts.aliyuncs.com", msgMap.GetValueOrDefault("HostId"));
            Assert.Equal("InvalidAccessKeyId.NotFound", msgMap.GetValueOrDefault("Code"));
#endif

            ex = await Assert.ThrowsAsync<CredentialException>(async () => { await supplier.GetAsync(); });
            msgMap = JsonConvert.DeserializeObject<Dictionary<string, object>>(ex.Message);
#if NET45
            Assert.NotNull(GetValueOrDefault(msgMap,"RequestId"));
            Assert.Equal("Specified access key is not found.", GetValueOrDefault(msgMap,"Message"));
            Assert.Equal("sts.aliyuncs.com", GetValueOrDefault(msgMap,"HostId"));
            Assert.Equal("InvalidAccessKeyId.NotFound", GetValueOrDefault(msgMap,"Code"));
#else
            Assert.NotNull(msgMap.GetValueOrDefault("RequestId"));
            Assert.Equal("Specified access key is not found.", msgMap.GetValueOrDefault("Message"));
            Assert.Equal("sts.aliyuncs.com", msgMap.GetValueOrDefault("HostId"));
            Assert.Equal("InvalidAccessKeyId.NotFound", msgMap.GetValueOrDefault("Code"));
#endif

            IAlibabaCloudCredentialsProvider innerProvider = new StaticAKCredentialsProvider.Builder()
                .AccessKeyId(config.AccessKeyId)
                .AccessKeySecret(config.AccessKeySecret)
                .Build();
            provider = new RamRoleArnCredentialProvider.Builder()
                .CredentialsProvider(innerProvider)
                .DurationSeconds(config.RoleSessionExpiration)
                .RoleArn(config.RoleArn)
                .RoleSessionName(config.RoleSessionName)
                .Policy(config.Policy)
                .STSEndpoint(config.STSEndpoint)
                .ExternalId(config.ExternalId)
                .ConnectTimeout(config.ConnectTimeout)
                .ReadTimeout(config.Timeout)
                .Build();
            supplier = new RefreshCachedSupplier<CredentialModel>(
                new Func<RefreshResult<CredentialModel>>(provider.RefreshCredentials),
                new Func<Task<RefreshResult<CredentialModel>>>(provider.RefreshCredentialsAsync));
            ex = Assert.Throws<CredentialException>(() => { supplier.Get(); });
            msgMap = JsonConvert.DeserializeObject<Dictionary<string, object>>(ex.Message);
#if NET45
            Assert.NotNull(GetValueOrDefault(msgMap,"RequestId"));
            Assert.Equal("Specified access key is not found.", GetValueOrDefault(msgMap,"Message"));
            Assert.Equal("sts.aliyuncs.com", GetValueOrDefault(msgMap,"HostId"));
            Assert.Equal("InvalidAccessKeyId.NotFound", GetValueOrDefault(msgMap,"Code"));
#else
            Assert.NotNull(msgMap.GetValueOrDefault("RequestId"));
            Assert.Equal("Specified access key is not found.", msgMap.GetValueOrDefault("Message"));
            Assert.Equal("sts.aliyuncs.com", msgMap.GetValueOrDefault("HostId"));
            Assert.Equal("InvalidAccessKeyId.NotFound", msgMap.GetValueOrDefault("Code"));
#endif
        }

        [Fact]
        public async Task TestNewRamRoleArnProvider()
        {
            Config config = new Config()
                { AccessKeyId = "accessKeyId", AccessKeySecret = "accessKeySecret", RoleArn = "roleArn" };
            RamRoleArnCredentialProvider provider = new RamRoleArnCredentialProvider(config);
            Assert.NotNull(provider);
            provider = new RamRoleArnCredentialProvider("accessKeyID", "accessKeySecret", "roleSessionName", "roleArn",
                "regionId", "policy");
            Assert.Throws<CredentialException>(() => { provider.GetCredentials(); });
            provider = new RamRoleArnCredentialProvider.Builder()
                .AccessKeyId("accessKeyID")
                .AccessKeySecret("accessKeySecret")
                .RoleSessionName("roleSessionName")
                .RoleArn("roleArn")
                .RegionId("regionId")
                .Policy("policy")
                .Build();
            Assert.Throws<CredentialException>(() => { provider.GetCredentials(); });

            Mock<IConnClient> mock = new Mock<IConnClient>();
            HttpResponse response = new HttpResponse("http://www.aliyun.com")
            {
                Status = 200,
                Encoding = "UTF-8",
                Content = Encoding.UTF8.GetBytes(
                    "{\"Credentials\":{\"Expiration\":\"2039-01-01T1:1:1Z\",\"AccessKeyId\":\"test\"," +
                    "\"AccessKeySecret\":\"test\",\"SecurityToken\":\"test\"}}")
            };
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(response);
            Assert.IsType<RefreshResult<CredentialModel>>(TestHelper.RunInstanceMethod(
                typeof(RamRoleArnCredentialProvider), "CreateCredential", provider, new object[] { mock.Object }));

            RefreshResult<CredentialModel> mockRefreshResult =
                (RefreshResult<CredentialModel>)TestHelper.RunInstanceMethod(typeof(RamRoleArnCredentialProvider),
                    "CreateCredential", provider, new object[] { mock.Object });

            var mockRefreshFunc = new Mock<Func<RefreshResult<CredentialModel>>>();
            mockRefreshFunc.Setup(f => f()).Returns(() => mockRefreshResult);

            var mockAsyncRefreshFunc = new Mock<Func<Task<RefreshResult<CredentialModel>>>>();
            mockAsyncRefreshFunc.Setup(f => f()).ReturnsAsync(mockRefreshResult);

            var supplier = new RefreshCachedSupplier<CredentialModel>(
                mockRefreshFunc.Object,
                mockAsyncRefreshFunc.Object);

            var resultAsync = await supplier.GetAsync();

            Assert.Equal("test", resultAsync.AccessKeyId);
            Assert.Equal("test", resultAsync.AccessKeySecret);
            Assert.Equal("test", resultAsync.SecurityToken);
            Assert.Equal("ram_role_arn/static_ak", resultAsync.ProviderName);
            Assert.Equal(2177456461000, resultAsync.Expiration);
            mockAsyncRefreshFunc.Verify(f => f(), Times.Once);

            // 返回未过期的cachedValue
            var result = supplier.Get();
            Assert.Equal("test", result.AccessKeyId);
            Assert.Equal("test", result.AccessKeySecret);
            Assert.Equal("test", result.SecurityToken);
            Assert.Equal(2177456461000, result.Expiration);

            response = new HttpResponse("http://www.aliyun.com")
            {
                Status = 200,
                Encoding = "UTF-8",
                Content = Encoding.UTF8.GetBytes(
                    "{\"Credentials\":{\"Expiration\":\"2019-01-01T1:1:1Z\",\"AccessKeyId\":\"test\"," +
                    "\"AccessKeySecret\":\"test\",\"SecurityToken\":\"test\"}}")
            };
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(response);
            Assert.IsType<RefreshResult<CredentialModel>>(TestHelper.RunInstanceMethod(
                typeof(RamRoleArnCredentialProvider), "CreateCredential", provider, new object[] { mock.Object }));

            mockRefreshResult = (RefreshResult<CredentialModel>)TestHelper.RunInstanceMethod(
                typeof(RamRoleArnCredentialProvider), "CreateCredential", provider, new object[] { mock.Object });

            mockRefreshFunc = new Mock<Func<RefreshResult<CredentialModel>>>();
            mockRefreshFunc.Setup(f => f()).Returns(() => mockRefreshResult);

            mockAsyncRefreshFunc = new Mock<Func<Task<RefreshResult<CredentialModel>>>>();
            mockAsyncRefreshFunc.Setup(f => f()).ReturnsAsync(mockRefreshResult);

            supplier = new RefreshCachedSupplier<CredentialModel>(
                mockRefreshFunc.Object,
                mockAsyncRefreshFunc.Object);

            var ex = await Assert.ThrowsAsync<CredentialException>(async () => { await supplier.GetAsync(); });
            Assert.Equal("No cached value was found.", ex.Message);

            ex = Assert.Throws<CredentialException>(() => { supplier.Get(); });
            Assert.Equal("No cached value was found.", ex.Message);
            mockRefreshFunc.Verify(f => f(), Times.Once);
        }

        [Fact]
        public void RamRoleArnProviderTest()
        {
            Config config = new Config()
                { AccessKeyId = "accessKeyId", AccessKeySecret = "accessKeySecret", RoleArn = "roleArn" };
            RamRoleArnCredentialProvider provider = new RamRoleArnCredentialProvider(config);
            Assert.NotNull(provider);

            provider = new RamRoleArnCredentialProvider("accessKeyID", "accessKeySecret", "roleSessionName", "roleArn",
                "regionId", "policy");
            Assert.Throws<CredentialException>(() => { provider.GetCredentials(); });

            Mock<IConnClient> mock = new Mock<IConnClient>();
            HttpResponse response = new HttpResponse("http://www.aliyun.com")
            {
                Status = 200,
                Encoding = "UTF-8",
                Content = Encoding.UTF8.GetBytes(
                    "{\"Credentials\":{\"Expiration\":\"2019-01-01T1:1:1Z\",\"AccessKeyId\":\"test\"," +
                    "\"AccessKeySecret\":\"test\",\"SecurityToken\":\"test\"}}")
            };
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(response);
            Assert.IsType<RefreshResult<CredentialModel>>(TestHelper.RunInstanceMethod(
                typeof(RamRoleArnCredentialProvider), "CreateCredential", provider, new object[] { mock.Object }));

            Mock<IAlibabaCloudCredentialsProvider> mockProvider = new Mock<IAlibabaCloudCredentialsProvider>();
            mockProvider.Setup(p => p.GetCredentials()).Returns(new CredentialModel
            {
                AccessKeyId = "accessKeyId",
                AccessKeySecret = "accessKeySecret",
                SecurityToken = "securityToken",
                Expiration = 64090527132000L
            });
            RamRoleArnCredential credentialMock = new RamRoleArnCredential("accessKeyId", "accessKeySecret",
                "securityToken", 1000L, mockProvider.Object);
            credentialMock.RefreshCredential();
            Assert.NotNull(credentialMock);
        }

        [Fact]
        public async Task RamRoleArnProviderAsyncTest()
        {
            Config config = new Config()
                { AccessKeyId = "accessKeyId", AccessKeySecret = "accessKeySecret", RoleArn = "roleArn" };
            RamRoleArnCredentialProvider provider = new RamRoleArnCredentialProvider(config);
            Assert.NotNull(provider);

            provider = new RamRoleArnCredentialProvider("accessKeyID", "accessKeySecret", "roleSessionName", "roleArn",
                "regionId", "policy");
            await Assert.ThrowsAsync<CredentialException>(async () => { await provider.RefreshCredentialsAsync(); });

            Mock<IConnClient> mock = new Mock<IConnClient>();
            HttpResponse response = new HttpResponse("http://www.aliyun.com")
            {
                Status = 200,
                Encoding = "UTF-8",
                Content = Encoding.UTF8.GetBytes(
                    "{\"Credentials\":{\"Expiration\":\"2019-01-01T1:1:1Z\",\"AccessKeyId\":\"test\"," +
                    "\"AccessKeySecret\":\"test\",\"SecurityToken\":\"test\"}}")
            };
            mock.Setup(p => p.DoActionAsync(It.IsAny<HttpRequest>())).ReturnsAsync(response);
            Assert.IsType<RefreshResult<CredentialModel>>(TestHelper.RunInstanceMethodAsync(
                typeof(RamRoleArnCredentialProvider), "CreateCredentialAsync", provider, new object[] { mock.Object }));

            Mock<IAlibabaCloudCredentialsProvider> mockProvider = new Mock<IAlibabaCloudCredentialsProvider>();
            mockProvider.Setup(p => p.GetCredentialsAsync()).ReturnsAsync(new CredentialModel
            {
                AccessKeyId = "accessKeyId",
                AccessKeySecret = "accessKeySecret",
                SecurityToken = "securityToken",
                Expiration = 64090527132000L
            });
            RamRoleArnCredential credentialMock = new RamRoleArnCredential("accessKeyId", "accessKeySecret",
                "securityToken", 1000L, mockProvider.Object);
            await credentialMock.RefreshCredentialAsync();
            Assert.NotNull(credentialMock);
        }

        [Fact]
        public async Task TestCacheIsStale()
        {
            // var staleTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - 1;
            var unixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var timeDiff = DateTimeOffset.UtcNow - unixEpoch;
            var staleTime = (long)timeDiff.TotalMilliseconds - 1;
            var refreshResult = new RefreshResult<CredentialModel>(new CredentialModel
            {
                AccessKeyId = "accessKeyId",
                AccessKeySecret = "accessKeySecret",
                SecurityToken = "securityToken",
                Expiration = 64090527132000L
                // }, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 10000);
            }, (long)(DateTimeOffset.UtcNow - unixEpoch).TotalMilliseconds + 10000);

            var mockSyncRefreshFunc = new Mock<Func<RefreshResult<CredentialModel>>>();
            mockSyncRefreshFunc.Setup(f => f()).Returns(() => refreshResult);

            var mockAsyncRefreshFunc = new Mock<Func<Task<RefreshResult<CredentialModel>>>>();
            mockAsyncRefreshFunc.Setup(f => f()).ReturnsAsync(refreshResult);

            var supplier = new RefreshCachedSupplier<CredentialModel>(
                mockSyncRefreshFunc.Object,
                mockAsyncRefreshFunc.Object);

            TestHelper.SetPrivateField(typeof(RefreshCachedSupplier<CredentialModel>), "cachedValue", supplier,
                new RefreshResult<CredentialModel>(new CredentialModel
                {
                    AccessKeyId = "accessKeyId",
                    AccessKeySecret = "accessKeySecret",
                    SecurityToken = "securityToken",
                    Expiration = 64090527132000L
                }, staleTime));

            var result = await supplier.GetAsync();

            Assert.Equal("accessKeyId", result.AccessKeyId);
            Assert.Equal("accessKeySecret", result.AccessKeySecret);
            Assert.Equal("securityToken", result.SecurityToken);
            Assert.Equal(64090527132000L, result.Expiration);
            mockAsyncRefreshFunc.Verify(f => f(), Times.Once);

            // 重置cachedValue
            TestHelper.SetPrivateField(typeof(RefreshCachedSupplier<CredentialModel>), "cachedValue", supplier,
                new RefreshResult<CredentialModel>(new CredentialModel
                {
                    AccessKeyId = "accessKeyId",
                    AccessKeySecret = "accessKeySecret",
                    SecurityToken = "securityToken",
                    Expiration = 64090527132000L
                }, staleTime));

            var result1 = supplier.Get();
            Assert.Equal("accessKeyId", result1.AccessKeyId);
            Assert.Equal("accessKeySecret", result1.AccessKeySecret);
            Assert.Equal("securityToken", result1.SecurityToken);
            Assert.Equal(64090527132000L, result1.Expiration);
            mockSyncRefreshFunc.Verify(f => f(), Times.Once);
        }
    }
}