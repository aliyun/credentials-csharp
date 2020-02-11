using System.Text;

using Aliyun.Credentials;
using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Http;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Provider;

using Moq;

using Xunit;

namespace aliyun_net_credentials_unit_tests.Provider
{
    public class RamRoleArnCredentialProviderTest
    {
        [Fact]
        public void RamRoleArnProviderTest()
        {
            Config config = new Config() { AccessKeyId = "accessKeyId", AccessKeySecret = "accessKeySecret", RoleArn = "roleArn" };
            RamRoleArnCredentialProvider provider = new RamRoleArnCredentialProvider(config);
            Assert.NotNull(provider);

            provider = new RamRoleArnCredentialProvider("accessKeyID", "accessKeySecret", "roleSessionName", "roleArn", "regionId", "policy");
            Assert.Throws<CredentialException>(() => { provider.GetCredentials(); });

            Mock<IConnClient> mock = new Mock<IConnClient>();
            HttpResponse response = new HttpResponse("http://www.aliyun.com")
            {
                Status = 200, Encoding = "UTF-8", Content = Encoding.UTF8.GetBytes("{\"Credentials\":{\"Expiration\":\"2019-01-01T1:1:1Z\",\"AccessKeyId\":\"test\"," +
                "\"AccessKeySecret\":\"test\",\"SecurityToken\":\"test\"}}")
            };
            mock.Setup(p => p.DoAction(It.IsAny<HttpRequest>())).Returns(response);
            Assert.IsType<RamRoleArnCredential>(TestHelper.RunInstanceMethod(typeof(RamRoleArnCredentialProvider), "CreateCredential", provider, new object[] { mock.Object }));

            Mock<IAlibabaCloudCredentialsProvider> mockProvider = new Mock<IAlibabaCloudCredentialsProvider>();
            mockProvider.Setup(p => p.GetCredentials()).Returns(new RamRoleArnCredential("accessKeyId", "accessKeySecret", "securityToken", 64090527132000L, null));
            RamRoleArnCredential credentialMock = new RamRoleArnCredential("accessKeyId", "accessKeySecret", "securityToken", 1000L, mockProvider.Object);
            credentialMock.RefreshCredential();
            Assert.NotNull(credentialMock);
        }
    }
}
