namespace Aliyun.Credentials.Http
{
    public class CompatibleUrlConnClient :IConnClient
    {
        public HttpResponse DoAction(HttpRequest request)
        {
            var response = GetResponse(request);
            return response;
        }

        private HttpResponse GetResponse(HttpRequest httpRequest)
        {
            return HttpResponse.GetResponse(httpRequest);
        }

    }
}
