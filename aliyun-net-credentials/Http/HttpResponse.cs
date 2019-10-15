using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Utils;

namespace Aliyun.Credentials.Http
{
    public class HttpResponse : HttpRequest
    {
        // Default read timeout 10s
        private const int DefaultReadTimeoutInMilliSeconds = 10000;

        // Default connect timeout 5s
        private const int DefaultConnectTimeoutInMilliSeconds = 5000;
        private const int BufferLength = 1024;

        public HttpResponse(string strUrl) : base(strUrl) { }

        public int Status { get; set; }

        public void SetContent(byte[] content, string encoding, FormatType? format)
        {
            Content = content;
            Encoding = encoding;
            ContentType = format;
        }

        private static void ParseHttpResponse(HttpResponse httpResponse, HttpWebResponse httpWebResponse)
        {
            httpResponse.Content = ReadContent(httpWebResponse);
            httpResponse.Status = (int) httpWebResponse.StatusCode;
            httpResponse.Headers = new Dictionary<string, string>();
            httpResponse.Method = ParameterHelper.StringToMethodType(httpWebResponse.Method);

            foreach (var key in httpWebResponse.Headers.AllKeys)
            {
                httpResponse.Headers.Add(key, httpWebResponse.Headers[key]);
            }

            var contentType = DictionaryUtil.Get(httpResponse.Headers, "Content-Type");

            if (null == contentType) return;
            httpResponse.Encoding = "UTF-8";
            var split = contentType.Split(';');
            httpResponse.ContentType = ParameterHelper.StingToFormatType(split[0].Trim());
            if (split.Length <= 1 || !split[1].Contains("=")) return;
            var codings = split[1].Split('=');
            httpResponse.Encoding = codings[1].Trim().ToUpper();
        }

        private static byte[] ReadContent(WebResponse rsp)
        {
            using(var ms = new MemoryStream())
            using(var stream = rsp.GetResponseStream())
            {
                {
                    var buffer = new byte[BufferLength];
                    while (stream != null)
                    {
                        var length = stream.Read(buffer, 0, BufferLength);
                        if (length == 0)
                        {
                            break;
                        }

                        ms.Write(buffer, 0, length);

                    }

                    ms.Seek(0, SeekOrigin.Begin);
                    var bytes = new byte[ms.Length];
                    ms.Read(bytes, 0, bytes.Length);
                    return bytes;
                }
            }
        }

        public static HttpResponse GetResponse(HttpRequest request, int? timeout = null)
        {
            CheckHttpRequest(request);
            var httpWebRequest = GetWebRequest(request);

            if (timeout != null)
            {
                httpWebRequest.Timeout = (int) timeout;
            }

            HttpWebResponse httpWebResponse;
            var httpResponse = new HttpResponse(httpWebRequest.RequestUri.AbsoluteUri);

            try
            {
                using(httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse())
                {
                    ParseHttpResponse(httpResponse, httpWebResponse);
                    return httpResponse;
                }
            }
            catch (WebException ex)
            {
                using(httpWebResponse = ex.Response as HttpWebResponse)
                {
                    ParseHttpResponse(httpResponse, httpWebResponse);
                    return httpResponse;
                }

            }
            catch (Exception ex)
            {
                throw new CredentialException("Exception",
                    string.Format("The request url is {0} {1}",
                        httpWebRequest.RequestUri == null ? "empty" : httpWebRequest.RequestUri.Host, ex));
            }
        }

        public static HttpWebRequest GetWebRequest(HttpRequest request)
        {
            var uri = new Uri(request.Url);
            var httpWebRequest = (HttpWebRequest) WebRequest.Create(uri);

            httpWebRequest.Method = request.Method.ToString();
            httpWebRequest.KeepAlive = true;

            httpWebRequest.Timeout =
                request.ConnectTimeout > 0 ? request.ConnectTimeout : DefaultConnectTimeoutInMilliSeconds;

            httpWebRequest.ReadWriteTimeout =
                request.ReadTimeout > 0 ? request.ReadTimeout : DefaultReadTimeoutInMilliSeconds;

            if (DictionaryUtil.Get(request.Headers, "Accept") != null)
            {
                httpWebRequest.Accept = DictionaryUtil.Pop(request.Headers, "Accept");
            }

            if (DictionaryUtil.Get(request.Headers, "Date") != null)
            {
                var headerDate = DictionaryUtil.Pop(request.Headers, "Date");
                httpWebRequest.Date = Convert.ToDateTime(headerDate);
            }

            foreach (var header in request.Headers)
            {
                if (header.Key.Equals("Content-Length"))
                {
                    httpWebRequest.ContentLength = long.Parse(header.Value);
                    continue;
                }

                if (header.Key.Equals("Content-Type"))
                {
                    httpWebRequest.ContentType = header.Value;
                    continue;
                }

                httpWebRequest.Headers.Add(header.Key, header.Value);
            }

            if ((request.Method != MethodType.Post && request.Method != MethodType.Put) || request.Content == null)
                return httpWebRequest;
            using(var stream = httpWebRequest.GetRequestStream())
            {
                stream.Write(request.Content, 0, request.Content.Length);
            }

            return httpWebRequest;
        }

        private static void CheckHttpRequest(HttpRequest request)
        {
            var strUrl = request.Url;
            if (null == strUrl)
            {
                throw new InvalidDataException("URL is null for HttpRequest.");
            }

            if (null == request.Method)
            {
                throw new InvalidDataException("Method is null for HttpRequest.");
            }
        }
    }
}
