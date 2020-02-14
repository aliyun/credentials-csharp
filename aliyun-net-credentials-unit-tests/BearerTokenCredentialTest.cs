using System.Threading.Tasks;

using Aliyun.Credentials;

using Xunit;

namespace aliyun_net_credentials_unit_tests
{
    public class BearerTokenCredentialTest
    {
        [Fact]
        public async Task TestBearerTokenCredential()
        {
            BearerTokenCredential bearerTokenCredential;
            bearerTokenCredential = new BearerTokenCredential("bearerToken");
            Assert.NotNull(bearerTokenCredential);
            Assert.Equal("bearerToken", bearerTokenCredential.GetBearerToken());
            Assert.Null(bearerTokenCredential.GetAccessKeyId());
            Assert.Null(bearerTokenCredential.GetAccessKeySecret());
            Assert.Null(bearerTokenCredential.GetSecurityToken());
            Assert.Null(bearerTokenCredential.GetCredentialType());

            Assert.Equal("bearerToken", await bearerTokenCredential.GetBearerTokenAsync());
            Assert.Null(await bearerTokenCredential.GetAccessKeyIdAsync());
            Assert.Null(await bearerTokenCredential.GetAccessKeySecretAsync());
            Assert.Null(await bearerTokenCredential.GetSecurityTokenAsync());
            Assert.Null(await bearerTokenCredential.GetCredentialTypeAsync());
        }
    }
}
