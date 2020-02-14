using System.Threading.Tasks;

namespace Aliyun.Credentials.Http
{
    public class CompatibleUrlConnClient : IConnClient
    {
        public HttpResponse DoAction(HttpRequest request)
        {
            var response = GetResponse(request);
            return response;
        }

        public async Task<HttpResponse> DoActionAsync(HttpRequest request)
        {
            var response = await GetResponseAsync(request);
            return response;
        }

        private HttpResponse GetResponse(HttpRequest httpRequest)
        {
            return HttpResponse.GetResponse(httpRequest);
        }

        private async Task<HttpResponse> GetResponseAsync(HttpRequest httpRequest)
        {
            return await HttpResponse.GetResponseAsync(httpRequest);
        }

    }
}
