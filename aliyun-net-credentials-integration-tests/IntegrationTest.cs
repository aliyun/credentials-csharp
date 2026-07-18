using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Http;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Provider;
using Aliyun.Credentials.Utils;

using Newtonsoft.Json;

using Moq;
using Xunit;
using Aliyun.Credentials;


namespace aliyun_net_credentials_unit_tests.Provider
{
    public class IntegrationTest
    {
        // The repository OIDC secrets can hit
        // AuthenticationFail.OIDCToken.PublicKeyFingerprintMismatch when the IdP
        // discovery fingerprint is invalid. That is an environment issue, not a
        // code defect, so skip instead of failing (same policy as credentials-go
        // and credentials-php integration tests).
        private const string OIDCFingerprintMismatch = "AuthenticationFail.OIDCToken.PublicKeyFingerprintMismatch";

        [Fact]
        public void ItegrationOIDCProviderTest()
        {
            string roleArn = Environment.GetEnvironmentVariable("ALIBABA_CLOUD_ROLE_ARN");
            string providerArn = Environment.GetEnvironmentVariable("ALIBABA_CLOUD_OIDC_PROVIDER_ARN");
            string tokenFilePath = Environment.GetEnvironmentVariable("ALIBABA_CLOUD_OIDC_TOKEN_FILE");

            if (string.IsNullOrEmpty(roleArn) || string.IsNullOrEmpty(providerArn) || string.IsNullOrEmpty(tokenFilePath))
            {
                return;
            }

            Config config = new Config
            {
                RoleArn = roleArn,
                OIDCProviderArn = providerArn,
                OIDCTokenFilePath = tokenFilePath,
                RoleSessionName = "test",
                Type = AuthConstant.OIDCRoleArn
            };

            Client client = new Client(config);
            CredentialModel credential;
            try
            {
                credential = client.GetCredential();
            }
            catch (CredentialException ex) when (ex.Message != null && ex.Message.Contains(OIDCFingerprintMismatch))
            {
                return;
            }
            Assert.NotNull(credential.AccessKeyId);
            Assert.NotNull(credential.AccessKeySecret);
            Assert.NotNull(credential.SecurityToken);
            Assert.Equal(AuthConstant.OIDCRoleArn, credential.Type);

            config = new Config
            {
                RoleSessionName = "test",
                Type = AuthConstant.OIDCRoleArn
            };

            client = new Client(config);
            credential = client.GetCredential();
            Assert.NotNull(credential.AccessKeyId);
            Assert.NotNull(credential.AccessKeySecret);
            Assert.NotNull(credential.SecurityToken);
            Assert.Equal(AuthConstant.OIDCRoleArn, credential.Type);
        }
    }
}