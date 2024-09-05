using System;
using System.Reflection;
using System.Threading.Tasks;

using Aliyun.Credentials;
using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Provider;
using Aliyun.Credentials.Utils;

using Moq;

using Xunit;

namespace aliyun_net_credentials_unit_tests.Provider
{
    class CredentialsProviderForTest : IAlibabaCloudCredentialsProvider
    {
        public CredentialModel GetCredentials()
        {
            return new CredentialModel
            {
                AccessKeyId = "",
                AccessKeySecret = "",
                Type = AuthConstant.AccessKey
            };
        }

        public Task<CredentialModel> GetCredentialsAsync()
        {
            return Task.Run(() => new CredentialModel
            {
                AccessKeyId = "",
                AccessKeySecret = "",
                Type = AuthConstant.AccessKey
            });
        }
    }

    public class DefaultCredentialsProviderTest
    {
        [Fact]
        public void DefaultProviderTest()
        {
            DefaultCredentialsProvider provider = new DefaultCredentialsProvider(false);
            Assert.NotNull(provider);
            Assert.Throws<CredentialException>(() => { provider.GetCredentials(); });

            RamRoleArnCredentialProvider testProvider = new RamRoleArnCredentialProvider("accessKeyId2", "accessKeySecret", "roleArn");
            provider.AddCredentialsProvider(testProvider);
            provider.RemoveCredentialsProvider(testProvider);
            Assert.False(provider.ContainsCredentialsProvider(testProvider));
            provider.ClearCredentialsProvider();

            Mock<IAlibabaCloudCredentialsProvider> mockProvider = new Mock<IAlibabaCloudCredentialsProvider>();
            mockProvider.Setup(p => p.GetCredentials()).Returns((CredentialModel) null);
            provider.AddCredentialsProvider(mockProvider.Object);
            Assert.Throws<CredentialException>(() => { provider.GetCredentials(); });
            provider.ClearCredentialsProvider();

            mockProvider = new Mock<IAlibabaCloudCredentialsProvider>();
            mockProvider.Setup(p => p.GetCredentials()).Returns(new CredentialModel{
                AccessKeyId = "accessKeyId",
                AccessKeySecret = "accessKeySecret",
                Type = AuthConstant.AccessKey
            });
            provider.AddCredentialsProvider(mockProvider.Object);
            Assert.IsType<CredentialModel>(provider.GetCredentials());

            provider.ClearCredentialsProvider();
            Assert.Throws<CredentialException>(() => { provider.GetCredentials(); });

            AuthUtils.EnvironmentEcsMetaData = null;
            AuthUtils.EnvironmentCredentialsURI = "http://test";
            provider = new DefaultCredentialsProvider();
            var ex = Assert.Throws<CredentialException>(() => { provider.GetCredentials(); });
            Assert.Contains("URLCredentialProvider: Failed to connect Server: http://test", ex.Message);
            AuthUtils.EnvironmentCredentialsURI = null;
        }

        [Fact]
        public async Task DefaultProviderAsyncTest()
        {
            DefaultCredentialsProvider provider = new DefaultCredentialsProvider(false);
            Assert.NotNull(provider);
            await Assert.ThrowsAsync<CredentialException>(async() => { await provider.GetCredentialsAsync(); });

            RamRoleArnCredentialProvider testProvider = new RamRoleArnCredentialProvider("accessKeyId2", "accessKeySecret", "roleArn");
            provider.AddCredentialsProvider(testProvider);
            provider.RemoveCredentialsProvider(testProvider);
            Assert.False(provider.ContainsCredentialsProvider(testProvider));
            provider.ClearCredentialsProvider();

            Mock<IAlibabaCloudCredentialsProvider> mockProvider = new Mock<IAlibabaCloudCredentialsProvider>();
            mockProvider.Setup(p => p.GetCredentialsAsync()).ReturnsAsync((CredentialModel) null);
            provider.AddCredentialsProvider(mockProvider.Object);
            await Assert.ThrowsAsync<CredentialException>(async() => { await provider.GetCredentialsAsync(); });
            provider.ClearCredentialsProvider();

            mockProvider = new Mock<IAlibabaCloudCredentialsProvider>();
            mockProvider.Setup(p => p.GetCredentialsAsync()).ReturnsAsync(new CredentialModel{
                AccessKeyId = "accessKeyId",
                AccessKeySecret = "accessKeySecret",
                Type = AuthConstant.AccessKey
            });
            provider.AddCredentialsProvider(mockProvider.Object);
            Assert.IsType<CredentialModel>(await provider.GetCredentialsAsync());

            provider.ClearCredentialsProvider();
            await Assert.ThrowsAsync<CredentialException>(async() => { await provider.GetCredentialsAsync(); });

            AuthUtils.EnvironmentEcsMetaData = null;
            AuthUtils.EnvironmentCredentialsURI = "http://test";
            provider = new DefaultCredentialsProvider();
            var ex = await Assert.ThrowsAsync<CredentialException>(async () => { await provider.GetCredentialsAsync(); });
            Assert.Contains("URLCredentialProvider: Failed to connect Server: http://test", ex.Message);
            AuthUtils.EnvironmentCredentialsURI = null;
        }

        [Fact]
        public void ReuseLastProviderEnabledTest()
        {
            DefaultCredentialsProvider provider = new DefaultCredentialsProvider();
            AuthUtils.EnvironmentAccessKeyId = "accessKeyId";
            AuthUtils.EnvironmentAccesskeySecret = "accessKeySecret";
            CredentialModel credential = provider.GetCredentials();
            Assert.Equal("accessKeyId", credential.AccessKeyId);
            Assert.Equal("accessKeySecret", credential.AccessKeySecret);

            Type providerType = typeof(DefaultCredentialsProvider);
            FieldInfo providerField = providerType.GetField("lastUsedCredentialsProvider", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.True(providerField.GetValue(provider) is EnvironmentVariableCredentialsProvider);
            FieldInfo reuseEnableField = providerType.GetField("reuseLastProviderEnabled", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.True((bool)reuseEnableField.GetValue(provider));

            provider.AddCredentialsProvider(new CredentialsProviderForTest());
            credential = provider.GetCredentials();
            Assert.Equal("accessKeyId", credential.AccessKeyId);
            Assert.Equal("accessKeySecret", credential.AccessKeySecret);
            Assert.True(providerField.GetValue(provider) is EnvironmentVariableCredentialsProvider);
            Assert.True((bool)reuseEnableField.GetValue(provider));

            provider.ClearCredentialsProvider();

            provider = new DefaultCredentialsProvider(false);
            credential = provider.GetCredentials();
            Assert.Equal("accessKeyId", credential.AccessKeyId);
            Assert.Equal("accessKeySecret", credential.AccessKeySecret);
            Assert.True(providerField.GetValue(provider) is EnvironmentVariableCredentialsProvider);
            Assert.False((bool)reuseEnableField.GetValue(provider));

            provider.AddCredentialsProvider(new CredentialsProviderForTest());
            credential = provider.GetCredentials();
            Assert.Equal("", credential.AccessKeyId);
            Assert.Equal("", credential.AccessKeySecret);
            Assert.True(providerField.GetValue(provider) is CredentialsProviderForTest);
            Assert.False((bool)reuseEnableField.GetValue(provider));

            AuthUtils.EnvironmentAccessKeyId = null;
            AuthUtils.EnvironmentAccesskeySecret = null;
            provider.ClearCredentialsProvider();
        }
    }
}
