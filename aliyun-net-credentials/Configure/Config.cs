namespace Aliyun.Credentials.Configure
{
    public static class Constants
    {
        public const string DefaultProfileName = "default";
        public const string StsDefaultEndpoint = "sts.aliyuncs.com";
        public const string DomainSuffix = "aliyuncs.com";
        public const string DefaultRegion = "cn-hangzhou";
        public const string ConfigStorePath = ".aliyun";
        public const string EnvPrefix = "ALIBABA_CLOUD_";

        public const string ECSIMDSSecurityCredURL =
            "http://100.100.100.200/latest/meta-data/ram/security-credentials/";

        public const string ECSIMDSSecurityCredTokenURL = "http://100.100.100.200/latest/api/token";
        public const string ECSIMDSHeaderPrefix = "X-aliyun-";
        public const string PATHCredentialFile = ".alibabacloud";
        public const string CloudMarkerUpperCaseForSub = "ALICLOUD";
    }
}