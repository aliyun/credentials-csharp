using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Utils;

using Newtonsoft.Json;

namespace Aliyun.Credentials.Provider
{
    public delegate void ExternalCredentialUpdateCallback(string accessKeyId, string accessKeySecret,
        string securityToken, long expiration);

    public class ExternalCredentialProvider : IAlibabaCloudCredentialsProvider
    {
        private const long ExpirationSlotSeconds = 180;
        private readonly string processCommand;
        private readonly int timeoutMilliseconds;
        private readonly ExternalCredentialUpdateCallback credentialUpdateCallback;
        private readonly object refreshLock = new object();
        private volatile CredentialModel credential;
        private long expirationTimestamp;

        private ExternalCredentialProvider(Builder builder)
        {
            if (string.IsNullOrEmpty(builder.processCommand))
            {
                throw new CredentialException("process_command is empty");
            }

            this.processCommand = builder.processCommand;
            this.timeoutMilliseconds = builder.timeoutMilliseconds ?? 60 * 1000;
            this.credentialUpdateCallback = builder.credentialUpdateCallback;
        }

        public class Builder
        {
            internal string processCommand;
            internal int? timeoutMilliseconds;
            internal ExternalCredentialUpdateCallback credentialUpdateCallback;

            public Builder ProcessCommand(string processCommand)
            {
                this.processCommand = processCommand;
                return this;
            }

            public Builder TimeoutMilliseconds(int? timeoutMilliseconds)
            {
                this.timeoutMilliseconds = timeoutMilliseconds;
                return this;
            }

            public Builder CredentialUpdateCallback(ExternalCredentialUpdateCallback callback)
            {
                this.credentialUpdateCallback = callback;
                return this;
            }

            public ExternalCredentialProvider Build()
            {
                return new ExternalCredentialProvider(this);
            }
        }

        public CredentialModel GetCredentials()
        {
            if (NeedUpdateCredential())
            {
                lock (refreshLock)
                {
                    if (NeedUpdateCredential())
                    {
                        CredentialModel refreshed = GetCredentialsInternal();
                        this.credential = refreshed;
                        this.expirationTimestamp = refreshed.Expiration > 0 ? refreshed.Expiration / 1000 : 0;
                        InvokeCredentialUpdateCallback(refreshed);
                    }
                }
            }

            return new CredentialModel
            {
                AccessKeyId = this.credential.AccessKeyId,
                AccessKeySecret = this.credential.AccessKeySecret,
                SecurityToken = this.credential.SecurityToken,
                Type = this.credential.Type,
                Expiration = this.credential.Expiration,
                ProviderName = GetProviderName()
            };
        }

        public Task<CredentialModel> GetCredentialsAsync()
        {
            return Task.FromResult(GetCredentials());
        }

        internal CredentialModel GetCredentialsInternal()
        {
            string[] args = Regex.Split(this.processCommand.Trim(), "\\s+");
            if (args.Length == 0 || string.IsNullOrEmpty(args[0]))
            {
                throw new CredentialException("process_command is empty");
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = args[0],
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            for (int i = 1; i < args.Length; i++)
            {
                startInfo.Arguments += (i > 1 ? " " : "") + args[i];
            }

            try
            {
                using (Process process = Process.Start(startInfo))
                {
                    if (!process.WaitForExit(this.timeoutMilliseconds))
                    {
                        process.Kill();
                        throw new CredentialException(string.Format(
                            "command process timed out after {0} milliseconds", this.timeoutMilliseconds));
                    }
                    string stdout = process.StandardOutput.ReadToEnd();
                    string stderr = process.StandardError.ReadToEnd();
                    if (process.ExitCode != 0)
                    {
                        throw new CredentialException(string.Format(
                            "failed to execute external command: exit status {0}\nstderr: {1}",
                            process.ExitCode, stderr));
                    }
                    return ParseCredentialResponse(stdout);
                }
            }
            catch (CredentialException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CredentialException("failed to execute external command: " + ex.Message, ex);
            }
        }

        private CredentialModel ParseCredentialResponse(string stdout)
        {
            Dictionary<string, object> response;
            try
            {
                response = JsonConvert.DeserializeObject<Dictionary<string, object>>(stdout);
            }
            catch (Exception ex)
            {
                throw new CredentialException("failed to parse external command output: " + ex.Message, ex);
            }

            string accessKeyId = GetString(response, "access_key_id");
            string accessKeySecret = GetString(response, "access_key_secret");
            string securityToken = GetString(response, "sts_token");
            string mode = GetString(response, "mode");
            string expirationStr = GetString(response, "expiration");

            if (string.IsNullOrEmpty(accessKeyId) || string.IsNullOrEmpty(accessKeySecret))
            {
                throw new CredentialException("invalid credential response: access_key_id or access_key_secret is empty");
            }
            if (mode == "StsToken" && string.IsNullOrEmpty(securityToken))
            {
                throw new CredentialException("invalid StsToken credential response: sts_token is empty");
            }

            long expiration = ParseExpiration(expirationStr);
            return new CredentialModel
            {
                AccessKeyId = accessKeyId,
                AccessKeySecret = accessKeySecret,
                SecurityToken = securityToken,
                Expiration = expiration,
                Type = string.IsNullOrEmpty(securityToken) ? AuthConstant.AccessKey : AuthConstant.Sts,
                ProviderName = GetProviderName()
            };
        }

        private static string GetString(Dictionary<string, object> values, string key)
        {
            if (values == null || !values.ContainsKey(key) || values[key] == null)
            {
                return null;
            }
            return values[key].ToString();
        }

        private static long ParseExpiration(string expiration)
        {
            if (string.IsNullOrEmpty(expiration))
            {
                return 0;
            }
            DateTime dateTime;
            if (!DateTime.TryParseExact(expiration, "yyyy-MM-dd'T'HH:mm:ss'Z'",
                CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out dateTime))
            {
                return 0;
            }
            return dateTime.GetTimeMillis();
        }

        private bool NeedUpdateCredential()
        {
            if (this.credential == null)
            {
                return true;
            }
            if (this.expirationTimestamp == 0)
            {
                return true;
            }
            return this.expirationTimestamp - DateTimeOffset.UtcNow.ToUnixTimeSeconds() <= ExpirationSlotSeconds;
        }

        private void InvokeCredentialUpdateCallback(CredentialModel refreshed)
        {
            if (this.credentialUpdateCallback == null)
            {
                return;
            }
            try
            {
                this.credentialUpdateCallback(refreshed.AccessKeyId, refreshed.AccessKeySecret,
                    refreshed.SecurityToken, this.expirationTimestamp);
            }
            catch (Exception)
            {
                // Warning only
            }
        }

        public string GetProviderName()
        {
            return "external";
        }
    }
}
