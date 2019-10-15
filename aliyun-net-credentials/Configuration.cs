namespace Aliyun.Credentials
{
    public class Configuration
    {
        private string type = "default";

        public string Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
            }
        }

        public string AccessKeyId { get; set; }

        public string AccessKeySecret { get; set; }

        public string SecurityToken { get; set; }

        public string RoleName { get; set; }

        public string RoleArn { get; set; }

        public string RoleSessionName { get; set; }

        public string Host { get; set; }

        public string PublicKeyId { get; set; }

        public string PrivateKeyFile { get; set; }

        public int ReadTimeout { get; set; }

        public int ConnectTimeout { get; set; }

        public string CertFile { get; set; }

        public string CertPassword { get; set; }

        public string Proxy { get; set; }
    }
}
