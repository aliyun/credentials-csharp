using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

using Aliyun.Credentials.Http;

namespace Aliyun.Credentials.Utils
{
    public class ParameterHelper
    {
        private const string Iso8601DateFormat = "yyyy-MM-dd'T'HH:mm:ss'Z'";
        private const string Separator = "&";

        public static string FormatIso8601Date(DateTime date)
        {
            return date.ToUniversalTime()
                .ToString(Iso8601DateFormat, CultureInfo.CreateSpecificCulture("en-US"));
        }

        public static string GetRfc2616Date(DateTime datetime)
        {
            return datetime.ToUniversalTime().GetDateTimeFormats('r') [0];
        }

        public static string Md5Sum(byte[] buff)
        {
            using(MD5 md5 = new MD5CryptoServiceProvider())
            {
                var output = md5.ComputeHash(buff);
                return BitConverter.ToString(output).Replace("-", "");
            }
        }

        public static string Md5SumAndBase64(byte[] buff)
        {
            using(MD5 md5 = new MD5CryptoServiceProvider())
            {
                var output = md5.ComputeHash(buff);
                return Convert.ToBase64String(output, 0, output.Length);
            }
        }

        public static string FormatTypeToString(FormatType? formatType)
        {
            switch (formatType)
            {
                case FormatType.Xml:
                    return "application/xml";
                case FormatType.Json:
                    return "application/json";
                case FormatType.Form:
                    return "application/x-www-form-urlencoded";
                default:
                    return "application/octet-stream";
            }
        }

        public static FormatType? StingToFormatType(string format)
        {
            switch (format.ToLower())
            {
                case "application/xml":
                case "text/xml":
                    return FormatType.Xml;
                case "application/json":
                    return FormatType.Json;
                case "application/x-www-form-urlencoded":
                    return FormatType.Form;
                default:
                    return FormatType.Raw;

            }
        }

        public static MethodType? StringToMethodType(string method)
        {
            method = method.ToUpper();
            switch (method)
            {
                case "GET":
                    return MethodType.GET;
                case "PUT":
                    return MethodType.PUT;
                case "POST":
                    return MethodType.POST;
                case "DELETE":
                    return MethodType.DELETE;
                case "HEAD":
                    return MethodType.HEAD;
                case "OPTIONS":
                    return MethodType.OPTIONS;
                default:
                    return null;
            }
        }

        public static string ComposeStringToSign(MethodType method, Dictionary<string, string> queries)
        {
            IDictionary<string, string> sortedDictionary =
                new SortedDictionary<string, string>(queries, StringComparer.Ordinal);

            var canonicalizedQueryString = new StringBuilder();
            foreach (var p in sortedDictionary)
            {
                canonicalizedQueryString.Append("&")
                    .Append(AcsURLEncoder.PercentEncode(p.Key)).Append("=")
                    .Append(AcsURLEncoder.PercentEncode(p.Value));
            }

            var stringToSign = new StringBuilder();
            stringToSign.Append(method.ToString());
            stringToSign.Append(Separator);
            stringToSign.Append(AcsURLEncoder.PercentEncode("/"));
            stringToSign.Append(Separator);
            stringToSign.Append(AcsURLEncoder.PercentEncode(
                canonicalizedQueryString.ToString().Substring(1)));

            return stringToSign.ToString();
        }

        public static string SignString(string source, string accessSecret)
        {
            using(KeyedHashAlgorithm algorithm = CryptoConfig.CreateFromName("HMACSHA1") as KeyedHashAlgorithm)
            {
                algorithm.Key = Encoding.UTF8.GetBytes(accessSecret.ToCharArray());
                return Convert.ToBase64String(algorithm.ComputeHash(Encoding.UTF8.GetBytes(source.ToCharArray())));
            }
        }

        public static string ComposeUrl(string endpoint, Dictionary<string, string> queries, string protocol)
        {
            Dictionary<string, string> mapQueries = queries;
            StringBuilder urlBuilder = new StringBuilder("");
            urlBuilder.Append(protocol);
            urlBuilder.Append("://").Append(endpoint);
            urlBuilder.Append("/?");
            StringBuilder builder = new StringBuilder("");
            foreach (var entry in mapQueries)
            {
                String key = entry.Key;
                String val = entry.Value;
                if (val == null)
                {
                    continue;
                }
                builder.Append(AcsURLEncoder.Encode(key));
                builder.Append("=").Append(AcsURLEncoder.Encode(val));
                builder.Append("&");
            }
            int strIndex = builder.Length;
            builder.Remove(strIndex - 1, 1);
            string query = builder.ToString();
            return urlBuilder.Append(query).ToString();
        }

        public static T ValidateNotNull<T>(T obj, string paramName, string message)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(paramName, message);
            }
            return obj;
        }
    }
}
