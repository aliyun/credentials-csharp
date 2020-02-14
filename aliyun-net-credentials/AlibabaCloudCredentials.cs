using System.Threading.Tasks;

namespace Aliyun.Credentials
{
    public interface IAlibabaCloudCredentials
    {
        string GetAccessKeyId();

        Task<string> GetAccessKeyIdAsync();

        string GetAccessKeySecret();

        Task<string> GetAccessKeySecretAsync();

        string GetSecurityToken();

        Task<string> GetSecurityTokenAsync();

        string GetCredentialType();

        Task<string> GetCredentialTypeAsync();
    }
}
