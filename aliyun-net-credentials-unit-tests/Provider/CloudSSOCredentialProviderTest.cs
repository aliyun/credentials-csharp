using System;
using System.Text;

using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Http;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Provider;
using Aliyun.Credentials.Utils;

using Moq;
using Xunit;

namespace aliyun_net_credentials_unit_tests.Provider
{
    public class CloudSSOCredentialProviderTest
    {
        [Fact]
        public void TestBuilderValidation()
        {
            var ex = Assert.Throws<CredentialException>(() =>
                new CloudSSOCredentialProvider.Builder()
                    .SignInUrl("https://signin.aliyuncs.com")
                    .AccountId("account123")
                    .AccessConfig("config123")
                    .Build());
            Assert.Contains("CloudSSO access token is empty or expired", ex.Message);

            ex = Assert.Throws<CredentialException>(() =>
                new CloudSSOCredentialProvider.Builder()
                    .SignInUrl("https://signin.aliyuncs.com")
                    .AccountId("account123")
                    .AccessConfig("config123")
                    .AccessToken("")
                    .AccessTokenExpire(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 1000)
                    .Build());
            Assert.Contains("CloudSSO access token is empty or expired", ex.Message);

            ex = Assert.Throws<CredentialException>(() =>
                new CloudSSOCredentialProvider.Builder()
                    .SignInUrl("https://signin.aliyuncs.com")
                    .AccountId("account123")
                    .AccessConfig("config123")
                    .AccessToken("token")
                    .AccessTokenExpire(1)
                    .Build());
            Assert.Contains("CloudSSO access token is empty or expired", ex.Message);

            ex = Assert.Throws<CredentialException>(() =>
                new CloudSSOCredentialProvider.Builder()
                    .AccessToken("token")
                    .AccessTokenExpire(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 1000)
                    .Build());
            Assert.Contains("CloudSSO sign in url, account id, and access config cannot be empty", ex.Message);

            ex = Assert.Throws<CredentialException>(() =>
                new CloudSSOCredentialProvider.Builder()
                    .SignInUrl("https://signin.aliyuncs.com")
                    .AccessToken("token")
                    .AccessTokenExpire(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 1000)
                    .Build());
            Assert.Contains("CloudSSO sign in url, account id, and access config cannot be empty", ex.Message);

            ex = Assert.Throws<CredentialException>(() =>
                new CloudSSOCredentialProvider.Builder()
                    .SignInUrl("https://signin.aliyuncs.com")
                    .AccountId("account123")
                    .AccessToken("token")
                    .AccessTokenExpire(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 1000)
                    .Build());
            Assert.Contains("CloudSSO sign in url, account id, and access config cannot be empty", ex.Message);
        }

        [Fact]
        public void TestGetNewSessionCredentials()
        {
            CloudSSOCredentialProvider provider = new CloudSSOCredentialProvider.Builder()
                .SignInUrl("https://signin.aliyuncs.com/saml/sso")
                .AccountId("account123")
                .AccessConfig("config123")
                .AccessToken("token")
                .AccessTokenExpire(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 1000)
                .Build();

            Mock<IConnClient> mock = new Mock<IConnClient>();
            HttpResponse response = new HttpResponse("https://signin.aliyuncs.com/cloud-credentials")
            {
                Status = 200,
                Encoding = "UTF-8",
                Content = Encoding.UTF8.GetBytes(
                    "{\"CloudCredential\":{\"AccessKeyId\":\"ak\",\"AccessKeySecret\":\"sk\"," +
                    "\"SecurityToken\":\"token\",\"Expiration\":\"2019-12-12T1:1:1Z\"}}")
            };
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(response);

            var result = (RefreshResult<CredentialModel>)TestHelper.RunInstanceMethod(
                typeof(CloudSSOCredentialProvider), "GetNewSessionCredentials", provider,
                new object[] { mock.Object });

            Assert.NotNull(result);
            Assert.Equal("ak", result.Value.AccessKeyId);
            Assert.Equal("sk", result.Value.AccessKeySecret);
            Assert.Equal("token", result.Value.SecurityToken);
            Assert.Equal(AuthConstant.Sts, result.Value.Type);
            Assert.Equal("cloud_sso", result.Value.ProviderName);
        }

        [Fact]
        public void TestGetNewSessionCredentialsAsync()
        {
            CloudSSOCredentialProvider provider = new CloudSSOCredentialProvider.Builder()
                .SignInUrl("https://signin.aliyuncs.com/saml/sso")
                .AccountId("account123")
                .AccessConfig("config123")
                .AccessToken("token")
                .AccessTokenExpire(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 1000)
                .Build();

            Mock<IConnClient> mock = new Mock<IConnClient>();
            HttpResponse response = new HttpResponse("https://signin.aliyuncs.com/cloud-credentials")
            {
                Status = 200,
                Encoding = "UTF-8",
                Content = Encoding.UTF8.GetBytes(
                    "{\"CloudCredential\":{\"AccessKeyId\":\"ak\",\"AccessKeySecret\":\"sk\"," +
                    "\"SecurityToken\":\"token\",\"Expiration\":\"2019-12-12T1:1:1Z\"}}")
            };
            mock.Setup(p => p.DoActionAsync(It.IsAny<HttpRequest>())).ReturnsAsync(response);

            var result = (RefreshResult<CredentialModel>)TestHelper.RunInstanceMethodAsync(
                typeof(CloudSSOCredentialProvider), "GetNewSessionCredentialsAsync", provider,
                new object[] { mock.Object });

            Assert.NotNull(result);
            Assert.Equal("ak", result.Value.AccessKeyId);
            Assert.Equal("sk", result.Value.AccessKeySecret);
            Assert.Equal("token", result.Value.SecurityToken);
            Assert.Equal(AuthConstant.Sts, result.Value.Type);
            Assert.Equal("cloud_sso", result.Value.ProviderName);
        }

        [Fact]
        public void TestGetNewSessionCredentialsError()
        {
            CloudSSOCredentialProvider provider = new CloudSSOCredentialProvider.Builder()
                .SignInUrl("https://signin.aliyuncs.com/saml/sso")
                .AccountId("account123")
                .AccessConfig("config123")
                .AccessToken("token")
                .AccessTokenExpire(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 1000)
                .Build();

            Mock<IConnClient> mock = new Mock<IConnClient>();
            HttpResponse response = new HttpResponse("https://signin.aliyuncs.com/cloud-credentials")
            {
                Status = 400,
                Encoding = "UTF-8",
                Content = Encoding.UTF8.GetBytes("Bad Request")
            };
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(response);

            var ex = Assert.Throws<CredentialException>(() =>
                TestHelper.RunInstanceMethod(typeof(CloudSSOCredentialProvider),
                    "GetNewSessionCredentials", provider, new object[] { mock.Object }));
            Assert.Contains("Get session token from CloudSSO failed, HttpCode: 400", ex.Message);
        }

        [Fact]
        public void TestGetNewSessionCredentialsMissingCloudCredential()
        {
            CloudSSOCredentialProvider provider = new CloudSSOCredentialProvider.Builder()
                .SignInUrl("https://signin.aliyuncs.com/saml/sso")
                .AccountId("account123")
                .AccessConfig("config123")
                .AccessToken("token")
                .AccessTokenExpire(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 1000)
                .Build();

            Mock<IConnClient> mock = new Mock<IConnClient>();
            HttpResponse response = new HttpResponse("https://signin.aliyuncs.com/cloud-credentials")
            {
                Status = 200,
                Encoding = "UTF-8",
                Content = Encoding.UTF8.GetBytes("{\"Other\":\"value\"}")
            };
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(response);

            var ex = Assert.Throws<CredentialException>(() =>
                TestHelper.RunInstanceMethod(typeof(CloudSSOCredentialProvider),
                    "GetNewSessionCredentials", provider, new object[] { mock.Object }));
            Assert.Contains("Get session token from CloudSSO failed, result:", ex.Message);
        }

        [Fact]
        public void TestGetNewSessionCredentialsMissingFields()
        {
            CloudSSOCredentialProvider provider = new CloudSSOCredentialProvider.Builder()
                .SignInUrl("https://signin.aliyuncs.com/saml/sso")
                .AccountId("account123")
                .AccessConfig("config123")
                .AccessToken("token")
                .AccessTokenExpire(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 1000)
                .Build();

            Mock<IConnClient> mock = new Mock<IConnClient>();
            HttpResponse response = new HttpResponse("https://signin.aliyuncs.com/cloud-credentials")
            {
                Status = 200,
                Encoding = "UTF-8",
                Content = Encoding.UTF8.GetBytes("{\"CloudCredential\":{\"AccessKeyId\":\"ak\"}}")
            };
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(response);

            var ex = Assert.Throws<CredentialException>(() =>
                TestHelper.RunInstanceMethod(typeof(CloudSSOCredentialProvider),
                    "GetNewSessionCredentials", provider, new object[] { mock.Object }));
            Assert.Contains("fail to get credentials", ex.Message);
        }

        [Fact]
        public void TestGetProviderName()
        {
            CloudSSOCredentialProvider provider = new CloudSSOCredentialProvider.Builder()
                .SignInUrl("https://signin.aliyuncs.com/saml/sso")
                .AccountId("account123")
                .AccessConfig("config123")
                .AccessToken("token")
                .AccessTokenExpire(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 1000)
                .Build();

            Assert.Equal("cloud_sso", provider.GetProviderName());
        }
    }
}
