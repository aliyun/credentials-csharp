using System;
using System.Collections.Generic;
using System.Net.Http;
using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Utils;

namespace Aliyun.Credentials.Http
{
    public class HttpRequest
    {
        private FormatType? contentType;

        public HttpRequest()
        {
            Headers = new Dictionary<string, string>();
            UrlParameters = new Dictionary<string, string>();
        }

        public HttpRequest(string strUrl)
        {
            Url = strUrl;
            Headers = new Dictionary<string, string>();
            UrlParameters = new Dictionary<string, string>();
        }

        public HttpRequest(string strUrl, Dictionary<string, string> tmpHeaders)
        {
            Url = strUrl;
            if (null != tmpHeaders)
            {
                Headers = tmpHeaders;
            }

            UrlParameters = new Dictionary<string, string>();
        }

        public void SetCommonUrlParameters()
        {
            DictionaryUtil.Add(UrlParameters, "Timestamp", ParameterHelper.FormatIso8601Date(DateTime.UtcNow));
            DictionaryUtil.Add(UrlParameters, "SignatureNonce", Guid.NewGuid().ToString());
            DictionaryUtil.Add(UrlParameters, "SignatureMethod", "HMAC-SHA1");
            DictionaryUtil.Add(UrlParameters, "SignatureVersion", "1.0");
        }

        public void AddUrlParameter(string key, string value)
        {
            DictionaryUtil.Add(UrlParameters, key, value);
        }

        public string GetHttpContentString()
        {
            string stringContent = string.Empty;
            if (this.Content != null)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(this.Encoding))
                    {
                        stringContent = Convert.ToBase64String(this.Content);
                    }
                    else
                    {
                        stringContent = System.Text.Encoding.GetEncoding(Encoding).GetString(this.Content);
                    }
                }
                catch
                {
                    throw new CredentialException("Can not parse response due to unsupported encoding.");
                }
            }

            return stringContent;
        }

        public void SetHttpContent(byte[] content, string encoding, FormatType? format)
        {
            if (content == null) {
                contentType = null;
                Content = null;
                Encoding = null;
                Headers = new Dictionary<string, string>()
                {
                    { "Content-Length", "0" }
                };
                return;
            }

            if (Method == MethodType.GET)
            {
                content = new byte[0];
            }

            Content = content;
            Encoding = encoding;
            string contentLen = content.Length.ToString();
            string strMd5 = ParameterHelper.Md5SumAndBase64(content);
            Headers = new Dictionary<string, string>()
            {
                { "Content-MD5", strMd5 },
                { "Content-Length", contentLen },
            };
            if (format != null)
            {
                Headers.Add("Content-Type", ParameterHelper.FormatTypeToString(format));
            }
        }

        public Dictionary<string, string> Headers { get; set; }
        public Dictionary<string, string> UrlParameters { get; set; }
        public string Url { get; set; }
        public MethodType? Method { get; set; }

        public FormatType? ContentType
        {
            get { return contentType; }
            set
            {
                if (value != null)
                {
                    this.contentType = value;
                }
            }
        }

        public byte[] Content { get; set; }
        public string Encoding { get; set; }

        public int ReadTimeout { get; set; }

        public int ConnectTimeout { get; set; }

    }
}
