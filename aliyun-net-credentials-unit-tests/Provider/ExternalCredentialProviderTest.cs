using System;
using System.IO;

using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Provider;

using Xunit;

namespace aliyun_net_credentials_unit_tests.Provider
{
    public class ExternalCredentialProviderTest
    {
        private static string CreateScript(string json)
        {
            string scriptPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".sh");
            File.WriteAllText(scriptPath, "#!/bin/sh\nprintf '%s\\n' '" + json.Replace("'", "'\\''") + "'\n");
            using (var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "/bin/chmod",
                    Arguments = "+x " + scriptPath,
                    UseShellExecute = false
                }
            })
            {
                process.Start();
                process.WaitForExit();
            }
            return scriptPath;
        }

        [Fact]
        public void TestBuilderValidation()
        {
            var ex = Assert.Throws<CredentialException>(() =>
                new ExternalCredentialProvider.Builder().Build());
            Assert.Equal("process_command is empty", ex.Message);
        }

        [Fact]
        public void TestGetCredentialsAK()
        {
            string scriptPath = CreateScript("{\"mode\":\"AK\",\"access_key_id\":\"ak\",\"access_key_secret\":\"sk\"}");
            var provider = new ExternalCredentialProvider.Builder()
                .ProcessCommand(scriptPath)
                .Build();

            var credential = provider.GetCredentials();
            Assert.Equal("ak", credential.AccessKeyId);
            Assert.Equal("sk", credential.AccessKeySecret);
            Assert.Null(credential.SecurityToken);
            Assert.Equal("external", credential.ProviderName);
        }

        [Fact]
        public void TestGetCredentialsStsTokenWithCallback()
        {
            string capturedToken = null;
            long capturedExpiration = 0;
            string scriptPath = CreateScript("{\"mode\":\"StsToken\",\"access_key_id\":\"ak\",\"access_key_secret\":\"sk\",\"sts_token\":\"token\",\"expiration\":\"2049-10-20T04:27:09Z\"}");
            var provider = new ExternalCredentialProvider.Builder()
                .ProcessCommand(scriptPath)
                .CredentialUpdateCallback((accessKeyId, accessKeySecret, securityToken, expiration) =>
                {
                    capturedToken = securityToken;
                    capturedExpiration = expiration;
                })
                .Build();

            var credential = provider.GetCredentials();
            Assert.Equal("ak", credential.AccessKeyId);
            Assert.Equal("sk", credential.AccessKeySecret);
            Assert.Equal("token", credential.SecurityToken);
            Assert.Equal("token", capturedToken);
            Assert.True(capturedExpiration > 0);
        }

        [Fact]
        public void TestRefreshEveryCallWithoutExpiration()
        {
            int callbackCount = 0;
            string scriptPath = CreateScript("{\"mode\":\"AK\",\"access_key_id\":\"ak\",\"access_key_secret\":\"sk\"}");
            var provider = new ExternalCredentialProvider.Builder()
                .ProcessCommand(scriptPath)
                .CredentialUpdateCallback((accessKeyId, accessKeySecret, securityToken, expiration) =>
                {
                    callbackCount++;
                })
                .Build();

            provider.GetCredentials();
            provider.GetCredentials();
            Assert.Equal(2, callbackCount);
        }

        [Fact]
        public void TestMissingStsToken()
        {
            string scriptPath = CreateScript("{\"mode\":\"StsToken\",\"access_key_id\":\"ak\",\"access_key_secret\":\"sk\"}");
            var provider = new ExternalCredentialProvider.Builder()
                .ProcessCommand(scriptPath)
                .Build();

            var ex = Assert.Throws<CredentialException>(() => provider.GetCredentials());
            Assert.Equal("invalid StsToken credential response: sts_token is empty", ex.Message);
        }

        [Fact]
        public void TestCallbackExceptionIgnored()
        {
            string scriptPath = CreateScript("{\"mode\":\"AK\",\"access_key_id\":\"ak\",\"access_key_secret\":\"sk\"}");
            var provider = new ExternalCredentialProvider.Builder()
                .ProcessCommand(scriptPath)
                .CredentialUpdateCallback((accessKeyId, accessKeySecret, securityToken, expiration) =>
                {
                    throw new Exception("callback error");
                })
                .Build();

            var credential = provider.GetCredentials();
            Assert.Equal("ak", credential.AccessKeyId);
        }
    }
}
