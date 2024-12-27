using System;
using System.Threading.Tasks;
using Aliyun.Credentials.Models;
// using Aliyun.Credentials.Policy;
using Aliyun.Credentials.Utils;

namespace Aliyun.Credentials.Provider
{
    public abstract class SessionCredentialsProvider : IAlibabaCloudCredentialsProvider
    {
        private readonly RefreshCachedSupplier<CredentialModel> credentialsCache;
        private readonly Func<RefreshResult<CredentialModel>> refreshFunc;
        private readonly Func<Task<RefreshResult<CredentialModel>>> refreshFuncAsync;
        private readonly bool asyncCredentialUpdateEnabled;

        public abstract RefreshResult<CredentialModel> RefreshCredentials();
        public abstract Task<RefreshResult<CredentialModel>> RefreshCredentialsAsync();
        
        protected SessionCredentialsProvider()
        {
            this.refreshFunc = RefreshCredentials;
            this.refreshFuncAsync = RefreshCredentialsAsync;
            this.credentialsCache = new RefreshCachedSupplier<CredentialModel>(refreshFunc, refreshFuncAsync);
        }
        
        protected SessionCredentialsProvider(Builder builder)
        {
            this.refreshFunc = RefreshCredentials;
            this.refreshFuncAsync = RefreshCredentialsAsync;
            this.asyncCredentialUpdateEnabled = builder.asyncCredentialUpdateEnabled;
            this.credentialsCache = new RefreshCachedSupplier<CredentialModel>.Builder(refreshFunc, refreshFuncAsync)
                .AsyncUpdateEnabled(builder.asyncCredentialUpdateEnabled)
                .StaleValueBehavior(builder.staleValueBehavior)
                .JitterEnabled(builder.jitterEnabled)
                .Build();
        }


        public CredentialModel GetCredentials()
        {
            return this.credentialsCache.Get();
        }

        public async Task<CredentialModel> GetCredentialsAsync()
        {
            return await this.credentialsCache.GetAsync();
        }

        public bool IsAsyncCredentialUpdateEnabled()
        {
            return this.asyncCredentialUpdateEnabled;
        }

        internal long GetStaleTime(long expiration)
        {
            var currentTimeMillis = DateTime.UtcNow.GetTimeMillis();
            return expiration <= 0 ? currentTimeMillis + 60 * 60 * 1000 : expiration - 15 * 60 * 1000;
        }
        
        public abstract string GetProviderName();
        
        public class Builder
        {
            internal bool asyncCredentialUpdateEnabled = false;
            internal bool jitterEnabled = true;
            internal Policy.StaleValueBehavior staleValueBehavior =
                Policy.StaleValueBehavior.Strict;

            public Builder JitterEnabled(bool buildJitterEnabled)
            {
                this.jitterEnabled = buildJitterEnabled;
                return this;
            }
            
            public Builder StaleValueBehavior(Policy.StaleValueBehavior buildStaleValueBehavior)
            {
                this.staleValueBehavior = buildStaleValueBehavior;
                return this;
            }
        }
    }
}