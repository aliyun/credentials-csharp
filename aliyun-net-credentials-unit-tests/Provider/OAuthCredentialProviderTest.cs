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
    public class OAuthCredentialProviderTest
    {
        [Fact]
        public void TestBuilderValidation()
        {
            var ex = Assert.Throws<CredentialException>(() =>
                new OAuthCredentialProvider.Builder()
                    .SignInUrl("https://oauth.aliyun.com")
                    .Build());
            Assert.Equal("The clientId is empty.", ex.Message);

            ex = Assert.Throws<CredentialException>(() =>
                new OAuthCredentialProvider.Builder()
                    .ClientId("")
                    .SignInUrl("https://oauth.aliyun.com")
                    .Build());
            Assert.Equal("The clientId is empty.", ex.Message);

            ex = Assert.Throws<CredentialException>(() =>
                new OAuthCredentialProvider.Builder()
                    .ClientId("client123")
                    .Build());
            Assert.Equal("The url for sign-in is empty.", ex.Message);

            ex = Assert.Throws<CredentialException>(() =>
                new OAuthCredentialProvider.Builder()
                    .ClientId("client123")
                    .SignInUrl("")
                    .Build());
            Assert.Equal("The url for sign-in is empty.", ex.Message);

            var provider = new OAuthCredentialProvider.Builder()
                .ClientId("client123")
                .SignInUrl("https://oauth.aliyun.com")
                .AccessToken("access_token")
                .AccessTokenExpire(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 1000)
                .Build();
            Assert.NotNull(provider);
        }

        [Fact]
        public void TestGetNewSessionCredentials()
        {
            OAuthCredentialProvider provider = new OAuthCredentialProvider.Builder()
                .ClientId("client123")
                .SignInUrl("https://oauth.aliyun.com")
                .AccessToken("access_token")
                .AccessTokenExpire(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 10000)
                .Build();

            Mock<IConnClient> mock = new Mock<IConnClient>();
            HttpResponse response = new HttpResponse("https://oauth.aliyun.com/v1/exchange")
            {
                Status = 200,
                Encoding = "UTF-8",
                Content = Encoding.UTF8.GetBytes(
                    "{\"accessKeyId\":\"ak\",\"accessKeySecret\":\"sk\"," +
                    "\"securityToken\":\"token\",\"expiration\":\"2019-12-12T1:1:1Z\"}")
            };
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(response);

            var result = (RefreshResult<CredentialModel>)TestHelper.RunInstanceMethod(
                typeof(OAuthCredentialProvider), "GetNewSessionCredentials", provider,
                new object[] { mock.Object });

            Assert.NotNull(result);
            Assert.Equal("ak", result.Value.AccessKeyId);
            Assert.Equal("sk", result.Value.AccessKeySecret);
            Assert.Equal("token", result.Value.SecurityToken);
            Assert.Equal(AuthConstant.Sts, result.Value.Type);
            Assert.Equal("oauth", result.Value.ProviderName);
        }

        [Fact]
        public void TestGetNewSessionCredentialsAsync()
        {
            OAuthCredentialProvider provider = new OAuthCredentialProvider.Builder()
                .ClientId("client123")
                .SignInUrl("https://oauth.aliyun.com")
                .AccessToken("access_token")
                .AccessTokenExpire(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 10000)
                .Build();

            Mock<IConnClient> mock = new Mock<IConnClient>();
            HttpResponse response = new HttpResponse("https://oauth.aliyun.com/v1/exchange")
            {
                Status = 200,
                Encoding = "UTF-8",
                Content = Encoding.UTF8.GetBytes(
                    "{\"accessKeyId\":\"ak\",\"accessKeySecret\":\"sk\"," +
                    "\"securityToken\":\"token\",\"expiration\":\"2019-12-12T1:1:1Z\"}")
            };
            mock.Setup(p => p.DoActionAsync(It.IsAny<HttpRequest>())).ReturnsAsync(response);

            var result = (RefreshResult<CredentialModel>)TestHelper.RunInstanceMethodAsync(
                typeof(OAuthCredentialProvider), "GetNewSessionCredentialsAsync", provider,
                new object[] { mock.Object });

            Assert.NotNull(result);
            Assert.Equal("ak", result.Value.AccessKeyId);
            Assert.Equal("sk", result.Value.AccessKeySecret);
            Assert.Equal("token", result.Value.SecurityToken);
            Assert.Equal(AuthConstant.Sts, result.Value.Type);
            Assert.Equal("oauth", result.Value.ProviderName);
        }

        [Fact]
        public void TestGetNewSessionCredentialsError()
        {
            OAuthCredentialProvider provider = new OAuthCredentialProvider.Builder()
                .ClientId("client123")
                .SignInUrl("https://oauth.aliyun.com")
                .AccessToken("access_token")
                .AccessTokenExpire(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 10000)
                .Build();

            Mock<IConnClient> mock = new Mock<IConnClient>();
            HttpResponse response = new HttpResponse("https://oauth.aliyun.com/v1/exchange")
            {
                Status = 400,
                Encoding = "UTF-8",
                Content = Encoding.UTF8.GetBytes("Bad Request")
            };
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(response);

            var ex = Assert.Throws<CredentialException>(() =>
                TestHelper.RunInstanceMethod(typeof(OAuthCredentialProvider),
                    "GetNewSessionCredentials", provider, new object[] { mock.Object }));
            Assert.Contains("Get session token from OAuth failed, HttpCode: 400", ex.Message);
        }

        [Fact]
        public void TestTokenRefreshTriggered()
        {
            OAuthCredentialProvider provider = new OAuthCredentialProvider.Builder()
                .ClientId("client123")
                .SignInUrl("https://oauth.aliyun.com")
                .RefreshToken("refresh_token_value")
                .AccessToken("old_access_token")
                .AccessTokenExpire(0)
                .Build();

            Mock<IConnClient> mock = new Mock<IConnClient>();

            HttpResponse refreshResponse = new HttpResponse("https://oauth.aliyun.com/v1/token")
            {
                Status = 200,
                Encoding = "UTF-8",
                Content = Encoding.UTF8.GetBytes(
                    "{\"access_token\":\"new_access_token\",\"refresh_token\":\"new_refresh_token\",\"expires_in\":3600}")
            };

            HttpResponse exchangeResponse = new HttpResponse("https://oauth.aliyun.com/v1/exchange")
            {
                Status = 200,
                Encoding = "UTF-8",
                Content = Encoding.UTF8.GetBytes(
                    "{\"accessKeyId\":\"ak\",\"accessKeySecret\":\"sk\"," +
                    "\"securityToken\":\"token\",\"expiration\":\"2019-12-12T1:1:1Z\"}")
            };

            int callCount = 0;
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>()))
                .Returns(() =>
                {
                    callCount++;
                    return callCount == 1 ? refreshResponse : exchangeResponse;
                });

            var result = (RefreshResult<CredentialModel>)TestHelper.RunInstanceMethod(
                typeof(OAuthCredentialProvider), "GetNewSessionCredentials", provider,
                new object[] { mock.Object });

            Assert.NotNull(result);
            Assert.Equal("ak", result.Value.AccessKeyId);
            Assert.Equal("sk", result.Value.AccessKeySecret);
            Assert.Equal("token", result.Value.SecurityToken);
            mock.Verify(p => p.DoAction(It.IsAny<HttpRequest>()), Times.Exactly(2));
        }

        [Fact]
        public void TestTokenRefreshError()
        {
            OAuthCredentialProvider provider = new OAuthCredentialProvider.Builder()
                .ClientId("client123")
                .SignInUrl("https://oauth.aliyun.com")
                .RefreshToken("refresh_token_value")
                .AccessToken("old_access_token")
                .AccessTokenExpire(0)
                .Build();

            Mock<IConnClient> mock = new Mock<IConnClient>();
            HttpResponse errorResponse = new HttpResponse("https://oauth.aliyun.com/v1/token")
            {
                Status = 401,
                Encoding = "UTF-8",
                Content = Encoding.UTF8.GetBytes("Unauthorized")
            };
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(errorResponse);

            var ex = Assert.Throws<CredentialException>(() =>
                TestHelper.RunInstanceMethod(typeof(OAuthCredentialProvider),
                    "GetNewSessionCredentials", provider, new object[] { mock.Object }));
            Assert.Contains("Failed to refresh OAuth token, status code: 401", ex.Message);
        }

        [Fact]
        public void TestTokenUpdateCallback()
        {
            bool callbackInvoked = false;
            string capturedAK = null;
            string capturedSK = null;

            OAuthCredentialProvider provider = new OAuthCredentialProvider.Builder()
                .ClientId("client123")
                .SignInUrl("https://oauth.aliyun.com")
                .AccessToken("access_token")
                .AccessTokenExpire(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 10000)
                .TokenUpdateCallback((refreshToken, accessToken, accessKeyId, accessKeySecret,
                    securityToken, accessTokenExpire, stsExpire) =>
                {
                    callbackInvoked = true;
                    capturedAK = accessKeyId;
                    capturedSK = accessKeySecret;
                })
                .Build();

            Mock<IConnClient> mock = new Mock<IConnClient>();
            HttpResponse response = new HttpResponse("https://oauth.aliyun.com/v1/exchange")
            {
                Status = 200,
                Encoding = "UTF-8",
                Content = Encoding.UTF8.GetBytes(
                    "{\"accessKeyId\":\"ak\",\"accessKeySecret\":\"sk\"," +
                    "\"securityToken\":\"token\",\"expiration\":\"2019-12-12T1:1:1Z\"}")
            };
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(response);

            TestHelper.RunInstanceMethod(typeof(OAuthCredentialProvider),
                "GetNewSessionCredentials", provider, new object[] { mock.Object });

            Assert.True(callbackInvoked);
            Assert.Equal("ak", capturedAK);
            Assert.Equal("sk", capturedSK);
        }

        [Fact]
        public void TestTokenUpdateCallbackException()
        {
            OAuthCredentialProvider provider = new OAuthCredentialProvider.Builder()
                .ClientId("client123")
                .SignInUrl("https://oauth.aliyun.com")
                .AccessToken("access_token")
                .AccessTokenExpire(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 10000)
                .TokenUpdateCallback((refreshToken, accessToken, accessKeyId, accessKeySecret,
                    securityToken, accessTokenExpire, stsExpire) =>
                {
                    throw new Exception("callback error");
                })
                .Build();

            Mock<IConnClient> mock = new Mock<IConnClient>();
            HttpResponse response = new HttpResponse("https://oauth.aliyun.com/v1/exchange")
            {
                Status = 200,
                Encoding = "UTF-8",
                Content = Encoding.UTF8.GetBytes(
                    "{\"accessKeyId\":\"ak\",\"accessKeySecret\":\"sk\"," +
                    "\"securityToken\":\"token\",\"expiration\":\"2019-12-12T1:1:1Z\"}")
            };
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(response);

            var result = (RefreshResult<CredentialModel>)TestHelper.RunInstanceMethod(
                typeof(OAuthCredentialProvider), "GetNewSessionCredentials", provider,
                new object[] { mock.Object });

            Assert.NotNull(result);
            Assert.Equal("ak", result.Value.AccessKeyId);
        }

        [Fact]
        public void TestGetNewSessionCredentialsMissingFields()
        {
            OAuthCredentialProvider provider = new OAuthCredentialProvider.Builder()
                .ClientId("client123")
                .SignInUrl("https://oauth.aliyun.com")
                .AccessToken("access_token")
                .AccessTokenExpire(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 10000)
                .Build();

            Mock<IConnClient> mock = new Mock<IConnClient>();
            HttpResponse response = new HttpResponse("https://oauth.aliyun.com/v1/exchange")
            {
                Status = 200,
                Encoding = "UTF-8",
                Content = Encoding.UTF8.GetBytes("{\"accessKeyId\":\"ak\"}")
            };
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(response);

            var ex = Assert.Throws<CredentialException>(() =>
                TestHelper.RunInstanceMethod(typeof(OAuthCredentialProvider),
                    "GetNewSessionCredentials", provider, new object[] { mock.Object }));
            Assert.Contains("Refresh session token from OAuth failed, fail to get credentials", ex.Message);
        }

        [Fact]
        public void TestGetProviderName()
        {
            OAuthCredentialProvider provider = new OAuthCredentialProvider.Builder()
                .ClientId("client123")
                .SignInUrl("https://oauth.aliyun.com")
                .AccessToken("access_token")
                .AccessTokenExpire(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 1000)
                .Build();

            Assert.Equal("oauth", provider.GetProviderName());
        }
    }
}
