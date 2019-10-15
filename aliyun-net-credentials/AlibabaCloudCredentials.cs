namespace Aliyun.Credentials
{
    public interface IAlibabaCloudCredentials
    {
        string AccessKeyId { get; }

        string AccessKeySecret { get; }

        string SecurityToken { get; }

        string CredentialType { get; }
    }
}
