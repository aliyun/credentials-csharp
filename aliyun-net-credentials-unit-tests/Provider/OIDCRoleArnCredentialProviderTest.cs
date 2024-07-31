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
using System.IO;
using Aliyun.Credentials.Utils;

namespace aliyun_net_credentials_unit_tests.Provider
{
    public class OIDCRoleArnCredentialProviderTest
    {
        [Fact]
        public void TestConstructor()
        {
            var nullEx = Assert.Throws<ArgumentNullException>(() => { new OIDCRoleArnCredentialProvider(null, null, null); });
            Assert.StartsWith("RoleArn must not be null.", nullEx.Message);
            OIDCRoleArnCredentialProvider provider = new OIDCRoleArnCredentialProvider("", "", "");
            var notExistEx = Assert.Throws<CredentialException>(() => { provider.GetCredentials(); });
            Assert.Equal("OIDCTokenFilePath  does not exist.", notExistEx.Message);
        }

        [Fact]
        public async void TestGetCredentials()
        {
            Config config = new Config
            {
                Policy = "test",
                RoleArn = "test",
                RoleSessionName = "test",
                OIDCProviderArn = "test",
                OIDCTokenFilePath = TestHelper.GetOIDCTokenFilePath(),
            };
            OIDCRoleArnCredentialProvider provider = new OIDCRoleArnCredentialProvider(config);
            var ex = Assert.Throws<CredentialException>(() => { provider.GetCredentials(); });
            var msgMap = JsonConvert.DeserializeObject<Dictionary<string, object>>(ex.Message);
            Assert.NotNull(msgMap.GetValueOrDefault("RequestId"));
            Assert.Equal("Parameter OIDCProviderArn is not valid", msgMap.GetValueOrDefault("Message"));
            Assert.Equal("sts.aliyuncs.com", msgMap.GetValueOrDefault("HostId"));
            Assert.Equal("AuthenticationFail.NoPermission", msgMap.GetValueOrDefault("Code"));

            ex = await Assert.ThrowsAsync<CredentialException>(async () => { await provider.GetCredentialsAsync(); });
            msgMap = JsonConvert.DeserializeObject<Dictionary<string, object>>(ex.Message);
            Assert.NotNull(msgMap.GetValueOrDefault("RequestId"));
            Assert.Equal("Parameter OIDCProviderArn is not valid", msgMap.GetValueOrDefault("Message"));
            Assert.Equal("sts.aliyuncs.com", msgMap.GetValueOrDefault("HostId"));
            Assert.Equal("AuthenticationFail.NoPermission", msgMap.GetValueOrDefault("Code"));

            var supplier = new RefreshCachedSupplier<CredentialModel>(new Func<RefreshResult<CredentialModel>>(provider.RefreshCredentials), new Func<Task<RefreshResult<CredentialModel>>>(provider.RefreshCredentialsAsync));
            ex = Assert.Throws<CredentialException>(() => { supplier.Get(); });
            Assert.NotNull(msgMap.GetValueOrDefault("RequestId"));
            Assert.Equal("Parameter OIDCProviderArn is not valid", msgMap.GetValueOrDefault("Message"));
            Assert.Equal("sts.aliyuncs.com", msgMap.GetValueOrDefault("HostId"));
            Assert.Equal("AuthenticationFail.NoPermission", msgMap.GetValueOrDefault("Code"));
        }

        [Fact]
        public async Task TestCreateCredentialAsync()
        {
            Config config = new Config
            {
                RoleArn = "test",
                OIDCProviderArn = "test",
                OIDCTokenFilePath = TestHelper.GetOIDCTokenFilePath(),
            };
            OIDCRoleArnCredentialProvider provider = new OIDCRoleArnCredentialProvider(config);

            Mock<IConnClient> mock = new Mock<IConnClient>();
            HttpResponse response = new HttpResponse("http://www.aliyun.com")
            {
                Status = 200,
                Encoding = "UTF-8",
                Content = Encoding.UTF8.GetBytes("{\"Credentials\":{\"Expiration\":\"2019-01-01T1:1:1Z\",\"AccessKeyId\":\"test\"," +
                "\"AccessKeySecret\":\"test\",\"SecurityToken\":\"test\"}}")
            };
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(response);
            Assert.IsType<RefreshResult<CredentialModel>>(TestHelper.RunInstanceMethod(typeof(OIDCRoleArnCredentialProvider), "CreateCredential", provider, new object[] { mock.Object }));
            RefreshResult<CredentialModel> mockRefreshResult = (RefreshResult<CredentialModel>)TestHelper.RunInstanceMethod(typeof(OIDCRoleArnCredentialProvider), "CreateCredential", provider, new object[] { mock.Object });
            Assert.Equal(AuthConstant.OIDCRoleArn, mockRefreshResult.Value.Type);

            mock.Setup(p => p.DoActionAsync(It.IsAny<HttpRequest>())).ReturnsAsync(response);
            Assert.IsType<RefreshResult<CredentialModel>>(TestHelper.RunInstanceMethodAsync(typeof(OIDCRoleArnCredentialProvider), "CreateCredentialAsync", provider, new object[] { mock.Object }));
            RefreshResult<CredentialModel> mockRefreshResultAsync = (RefreshResult<CredentialModel>)TestHelper.RunInstanceMethodAsync(typeof(OIDCRoleArnCredentialProvider), "CreateCredentialAsync", provider, new object[] { mock.Object });
            Assert.Equal(AuthConstant.OIDCRoleArn, mockRefreshResultAsync.Value.Type);
        }
    }
}