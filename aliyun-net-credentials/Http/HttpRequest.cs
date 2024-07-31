using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Utils;

namespace Aliyun.Credentials.Http
{
    public class HttpRequest
    {
        private FormatType? contentType;
        protected static readonly string UserAgent = "User-Agent";
        private static readonly string DefaultUserAgent;

        static HttpRequest()
        {
            DefaultUserAgent = GetDefaultUserAgent();
        }

        public HttpRequest()
        {
            Headers = new Dictionary<string, string>
            {
                { UserAgent, DefaultUserAgent }
            };
            UrlParameters = new Dictionary<string, string>();
        }

        public HttpRequest(string strUrl)
        {
            Url = strUrl;
            Headers = new Dictionary<string, string>
            {
                { UserAgent, DefaultUserAgent }
            };
            UrlParameters = new Dictionary<string, string>();
        }

        public HttpRequest(string strUrl, Dictionary<string, string> tmpHeaders)
        {
            Url = strUrl;
            if (null != tmpHeaders)
            {
                Headers = tmpHeaders;
                Headers[UserAgent] = DefaultUserAgent;
            }
            else
            {
                Headers = new Dictionary<string, string>
                {
                    { UserAgent, DefaultUserAgent }
                };
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
            if (content == null)
            {
                contentType = null;
                Content = null;
                Encoding = null;
                Headers.Remove("Content-MD5");
                Headers.Remove("Content-Type");
                Headers["Content-Length"] = "0";
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
            Headers["Content-MD5"] = strMd5;
            Headers["Content-Length"] = contentLen;
            if (format != null)
            {
                Headers["Content-Type"] = ParameterHelper.FormatTypeToString(format);
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

        internal static string GetDefaultUserAgent()
        {
            string osVersion = Environment.OSVersion.ToString();
            string clientVersion = GetRuntimeRegexValue(RuntimeEnvironment.GetRuntimeDirectory());
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            string defaultUserAgent = string.Format("AlibabaCloud ({0}) {1} Credentials/{2} TeaDSL/1",
                osVersion,
                clientVersion,
                version
            );
            return defaultUserAgent;
        }

        internal static string GetRuntimeRegexValue(string value)
        {
            var rx = new Regex(@"(\.NET).*(\\|\/).*(\d)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var matches = rx.Match(value);
            char[] separator = { '\\', '/' };

            if (matches.Success)
            {
                var clientValueArray = matches.Value.Split(separator);
                return BuildClientVersion(clientValueArray);
            }

            return "RuntimeNotFound";
        }

        internal static string BuildClientVersion(string[] value)
        {
            var finalValue = "";
            for (var i = 0; i < value.Length - 1; ++i)
            {
                finalValue += value[i].Replace(".", "").ToLower();
            }

            finalValue += "/" + value[value.Length - 1];
            return finalValue;
        }

        public byte[] Content { get; set; }
        public string Encoding { get; set; }

        public int ReadTimeout { get; set; }

        public int ConnectTimeout { get; set; }

    }
}
