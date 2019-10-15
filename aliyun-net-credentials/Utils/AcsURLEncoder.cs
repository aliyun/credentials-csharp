using System.Globalization;
using System.Text;
using System.Web;

namespace Aliyun.Credentials.Utils
{
    public class AcsURLEncoder
    {
        private const string EncodingUtf8 = "UTF-8";

        public static string Encode(string value)
        {
            return HttpUtility.UrlEncode(value, Encoding.UTF8);
        }

        public static string PercentEncode(string value)
        {
            var stringBuilder = new StringBuilder();
            var text = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";
            var bytes = Encoding.GetEncoding(EncodingUtf8).GetBytes(value);
            foreach (var b in bytes)
            {
                var c = (char) b;
                if (text.IndexOf(c) >= 0)
                {
                    stringBuilder.Append(c);
                }
                else
                {
                    stringBuilder.Append("%").Append(string.Format(CultureInfo.InvariantCulture, "{0:X2}", (int) c));
                }
            }

            return stringBuilder.ToString();
        }
    }
}
