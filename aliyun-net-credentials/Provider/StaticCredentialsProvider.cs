using System.Threading.Tasks;
using Aliyun.Credentials.Models;

namespace Aliyun.Credentials.Provider
{
    public class StaticCredentialsProvider : IAlibabaCloudCredentialsProvider
    {
        private readonly CredentialModel credential;

        public StaticCredentialsProvider(CredentialModel credential)
        {
            this.credential = credential;
        }

        public CredentialModel GetCredentials()
        {
            return credential;
        }

        public async Task<CredentialModel> GetCredentialsAsync()
        {
            return await Task.Run(() =>
            {
                return credential;
            });
        }
    }
}
