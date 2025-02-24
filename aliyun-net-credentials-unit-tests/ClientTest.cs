using System;
using System.Collections.Generic;
using Aliyun.Credentials;
using Aliyun.Credentials.Utils;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Provider;
using Aliyun.Credentials.Exceptions;

using Xunit;
using Newtonsoft.Json;

namespace aliyun_net_credentials_unit_tests
{
    public class ClientTest
    {
        [Fact]
        public void TestGetProvider()
        {
            Config config = new Config
            {
                Type = AuthConstant.AccessKey,
                RoleName = "test",
                AccessKeyId = null,
                AccessKeySecret = "AccessKeySecret"
            };

            var exception = Assert.Throws<ArgumentNullException>(() => new AccessKeyCredential(null, "test"));
            Assert.StartsWith("Access key ID cannot be null.", exception.Message);

            config.AccessKeyId = "AccessKeyId";
            Client client = new Client(config);
            config.Type = AuthConstant.EcsRamRole;
            var result = TestHelper.RunInstanceMethod(typeof(Client), "GetProvider", client, new object[] { config });
            Assert.IsType<EcsRamRoleCredentialProvider>(result);

            config.Type = AuthConstant.RamRoleArn;
            result = TestHelper.RunInstanceMethod(typeof(Client), "GetProvider", client, new object[] { config });
            Assert.IsType<RamRoleArnCredentialProvider>(result);

            config.Type = AuthConstant.RamRoleArn;
            result = TestHelper.RunInstanceMethod(typeof(Client), "GetProvider", client, new object[] { config });
            Assert.IsType<RamRoleArnCredentialProvider>(result);
            Assert.Equal("StaticAKCredentialsProvider", ((RamRoleArnCredentialProvider)result).CredentialsProvider.GetType().Name);

            config.Type = AuthConstant.RamRoleArn;
            config.SecurityToken = "test";
            result = TestHelper.RunInstanceMethod(typeof(Client), "GetProvider", client, new object[] { config });
            Assert.IsType<RamRoleArnCredentialProvider>(result);
            Assert.Equal("StaticSTSCredentialsProvider", ((RamRoleArnCredentialProvider)result).CredentialsProvider.GetType().Name);

            config.Type = AuthConstant.RsaKeyPair;
            config.PublicKeyId = "test";
            config.PrivateKeyFile = "/test";
            result = TestHelper.RunInstanceMethod(typeof(Client), "GetProvider", client, new object[] { config });
            Assert.IsType<RsaKeyPairCredentialProvider>(result);

            config.Type = AuthConstant.OIDCRoleArn;
            config.RoleArn = "test";
            config.OIDCProviderArn = "test";
            config.OIDCTokenFilePath = TestHelper.GetOIDCTokenFilePath();
            result = TestHelper.RunInstanceMethod(typeof(Client), "GetProvider", client, new object[] { config });
            Assert.IsType<OIDCRoleArnCredentialProvider>(result);

            config.Type = AuthConstant.URLSts;
            config.CredentialsURI = "http://test";
            result = TestHelper.RunInstanceMethod(typeof(Client), "GetProvider", client, new object[] { config });
            Assert.IsType<URLCredentialProvider>(result);

            config.Type = AuthConstant.CredentialsURI;
            config.CredentialsURI = "http://test";
            result = TestHelper.RunInstanceMethod(typeof(Client), "GetProvider", client, new object[] { config });
            Assert.IsType<URLCredentialProvider>(result);

            config.Type = null;
            var ex = Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethod(typeof(Client), "GetProvider", client, new object[] { config });
            });
            Assert.Equal("Unsupported credential type option: , support: access_key, sts, bearer, ecs_ram_role, ram_role_arn, rsa_key_pair, oidc_role_arn, credentials_uri", ex.Message);

            config.Type = "default";
            ex = Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethod(typeof(Client), "GetProvider", client, new object[] { config });
            });
            Assert.Equal("Unsupported credential type option: default, support: access_key, sts, bearer, ecs_ram_role, ram_role_arn, rsa_key_pair, oidc_role_arn, credentials_uri", ex.Message);

            config.Type = AuthConstant.Sts;
            config.SecurityToken = "test";
            result = TestHelper.RunInstanceMethod(typeof(Client), "GetProvider", client, new object[] { config });
            Assert.IsType<StaticCredentialsProvider>(result);
            Assert.Equal("static", ((StaticCredentialsProvider)result).GetProviderName());
        }

        [Fact]
        public void TestConstructor()
        {
            var ex = Assert.Throws<CredentialException>(() => new Client(new DefaultCredentialsProvider()).GetCredential());
            string[] expectedMessage = new[]
            {
                "not found credentials:",
                "[EnvironmentVariableCredentialsProvider: Environment variable accessKeyId cannot be empty,",
                "CLIProfileCredentialsProvider: Unable to open credentials file:",
                "ProfileCredentialsProvider: Unable to open credentials file:",
                "EcsRamRoleCredentialProvider: Failed to connect ECS Metadata Service: Aliyun.Credentials.Exceptions.CredentialException: Exception : The request url is 100.100.100.200"
            };

            foreach (var subMessage in expectedMessage)
            {
                Assert.Contains(subMessage, ex.Message);
            }

            var cacheMetadataDisabled = AuthUtils.EnvironmentEcsMetaDataDisabled;
            AuthUtils.EnvironmentEcsMetaDataDisabled = "true";
            ex = Assert.Throws<CredentialException>(() => new Client(new DefaultCredentialsProvider()).GetCredential());
            expectedMessage = new[]
            {
                "not found credentials:",
                "[EnvironmentVariableCredentialsProvider: Environment variable accessKeyId cannot be empty,",
                "CLIProfileCredentialsProvider: Unable to open credentials file:",
                "ProfileCredentialsProvider: Unable to open credentials file:",
            };

            foreach (var subMessage in expectedMessage)
            {
                Assert.Contains(subMessage, ex.Message);
            }

            AuthUtils.EnvironmentEcsMetaDataDisabled = cacheMetadataDisabled;

            ex = Assert.Throws<CredentialException>(() => new Client(new ProfileCredentialsProvider()).GetCredential());
            Assert.StartsWith("Unable to open credentials file:", ex.Message);

            ex = Assert.Throws<CredentialException>(() => new Client(new CLIProfileCredentialsProvider()).GetCredential());
            Assert.StartsWith("Unable to open credentials file:", ex.Message);

            ex = Assert.Throws<CredentialException>(() => new Client(new EcsRamRoleCredentialProvider("test")).GetCredential());
            Assert.StartsWith("Failed to connect ECS Metadata Service:", ex.Message);

            ex = Assert.Throws<CredentialException>(() => new Client(new EnvironmentVariableCredentialsProvider()).GetCredential());
            Assert.StartsWith("Environment variable accessKeyId cannot be empty", ex.Message);

            ex = Assert.Throws<CredentialException>(() => new Client(new OIDCRoleArnCredentialProvider("test", "test", "test")).GetCredential());
            Assert.StartsWith("OIDCTokenFilePath test does not exist.", ex.Message);

            ex = Assert.Throws<CredentialException>(() => new Client(new RamRoleArnCredentialProvider("test", "test", "test")).GetCredential());

            var credential = new Client(new StaticAKCredentialsProvider("test", "test")).GetCredential();
            Assert.Equal("test", credential.AccessKeyId);
            Assert.Equal("test", credential.AccessKeySecret);

            credential = new Client(new StaticSTSCredentialsProvider(new Config
            {
                AccessKeyId = "test",
                AccessKeySecret = "test",
                SecurityToken = "test"
            })).GetCredential();

            Assert.Equal("test", credential.AccessKeyId);
            Assert.Equal("test", credential.AccessKeySecret);
            Assert.Equal("test", credential.SecurityToken);

            ex = Assert.Throws<CredentialException>(() => new Client(new Client()).GetCredential());
            Assert.StartsWith("Ivalid initialization parameter", ex.Message);

            ex = Assert.Throws<CredentialException>(() => new Client().GetCredential());
            Assert.StartsWith("not found credentials", ex.Message);

            ex = Assert.Throws<CredentialException>(() => new Client(null).GetCredential());
            Assert.StartsWith("not found credentials", ex.Message);
            Assert.Contains("Unable to open credentials file: ", ex.Message);

            AuthUtils.EnvironmentEcsMetaData = "test";
            ex = Assert.Throws<CredentialException>(() => new Client(null).GetCredential());
            Assert.StartsWith("not found credentials", ex.Message);
            Assert.Contains("Failed to connect ECS Metadata Service: ", ex.Message);
            AuthUtils.EnvironmentEcsMetaData = null;

            AuthUtils.EnvironmentRoleArn = "role_arn";
            AuthUtils.EnvironmentOIDCProviderArn = "oidc_provider_arn";
            AuthUtils.EnvironmentOIDCTokenFilePath = TestHelper.GetOIDCTokenFilePath();
            ex = Assert.Throws<CredentialException>(() => new Client().GetCredential());
            string keyword = "OIDCRoleArnCredentialProvider:";
            int startIndex = ex.Message.IndexOf(keyword);
            int endIndex = ex.Message.IndexOf("}", startIndex);
            startIndex += keyword.Length;
            var msgMap = JsonConvert.DeserializeObject<Dictionary<string, object>>(ex.Message.Substring(startIndex, endIndex - startIndex + 1).Trim());
            Assert.NotNull(msgMap.GetValueOrDefault("RequestId"));
            Assert.Equal("Parameter OIDCProviderArn is not valid", msgMap.GetValueOrDefault("Message"));
            Assert.Equal(Aliyun.Credentials.Configure.Constants.StsDefaultEndpoint, msgMap.GetValueOrDefault("HostId"));
            Assert.Equal("AuthenticationFail.NoPermission", msgMap.GetValueOrDefault("Code"));
            AuthUtils.EnvironmentRoleArn = null;
            AuthUtils.EnvironmentOIDCProviderArn = null;
            AuthUtils.EnvironmentOIDCTokenFilePath = null;
        }
    }
}