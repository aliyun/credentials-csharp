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
        [Fact]
        public void ItegrationOIDCProviderTest()
        {
            string roleArn = Environment.GetEnvironmentVariable(Configure.Constants.EnvPrefix + "ROLE_ARN");
            string providerArn = Environment.GetEnvironmentVariable(Configure.Constants.EnvPrefix + "OIDC_PROVIDER_ARN");
            string tokenFilePath = Environment.GetEnvironmentVariable(Configure.Constants.EnvPrefix + "OIDC_TOKEN_FILE");

            Config config = new Config
            {
                RoleArn = roleArn,
                OIDCProviderArn = providerArn,
                OIDCTokenFilePath = tokenFilePath,
                RoleSessionName = "test",
                Type = AuthConstant.OIDCRoleArn
            };

            Client client = new Client(config);
            CredentialModel credential = client.GetCredential();
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