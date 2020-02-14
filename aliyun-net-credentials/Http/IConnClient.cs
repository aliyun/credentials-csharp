using System.Threading.Tasks;

namespace Aliyun.Credentials.Http
{
    public interface IConnClient
    {
        HttpResponse DoAction(HttpRequest request);

        Task<HttpResponse> DoActionAsync(HttpRequest request);
    }
}
