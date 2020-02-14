using System.Threading.Tasks;

namespace Aliyun.Credentials.Provider
{
    public interface IAlibabaCloudCredentialsProvider
    {
        IAlibabaCloudCredentials GetCredentials();

        Task<IAlibabaCloudCredentials> GetCredentialsAsync();
    }
}
