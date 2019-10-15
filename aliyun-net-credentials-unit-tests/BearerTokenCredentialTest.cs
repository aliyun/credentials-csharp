using Aliyun.Credentials;

using Xunit;

namespace aliyun_net_credentials_unit_tests
{
    public class BearerTokenCredentialTest
    {
        [Fact]
        public void TestBearerTokenCredential()
        {
            BearerTokenCredential bearerTokenCredential;
            bearerTokenCredential = new BearerTokenCredential("bearerToken");
            Assert.NotNull(bearerTokenCredential);
            Assert.Equal("bearerToken", bearerTokenCredential.BearerToken);
            Assert.Null(bearerTokenCredential.AccessKeyId);
            Assert.Null(bearerTokenCredential.AccessKeySecret);
            Assert.Null(bearerTokenCredential.SecurityToken);
            Assert.Null(bearerTokenCredential.CredentialType);
        }
    }
}
