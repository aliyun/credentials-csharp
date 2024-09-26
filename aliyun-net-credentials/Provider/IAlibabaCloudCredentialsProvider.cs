using System.Threading.Tasks;
using Aliyun.Credentials.Models;

namespace Aliyun.Credentials.Provider
{
    public interface IAlibabaCloudCredentialsProvider
    {
        CredentialModel GetCredentials();

        Task<CredentialModel> GetCredentialsAsync();

        string GetProviderName();
    }
}
