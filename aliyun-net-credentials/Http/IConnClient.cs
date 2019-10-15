namespace Aliyun.Credentials.Http
{
    public interface IConnClient
    {
        HttpResponse DoAction(HttpRequest request);
    }
}
