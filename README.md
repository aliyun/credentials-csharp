English | [简体中文](./README-CN.md)

![](https://aliyunsdk-pages.alicdn.com/icons/AlibabaCloud.svg)

## Alibaba Cloud Credentials for .NET

[![.NET CI](https://github.com/aliyun/credentials-csharp/actions/workflows/ci.yml/badge.svg)](https://github.com/aliyun/credentials-csharp/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/aliyun/credentials-csharp/graph/badge.svg?token=gZjatna6gL)](https://codecov.io/gh/aliyun/credentials-csharp)
[![Nuget Version](https://badge.fury.io/nu/Aliyun.Credentials.svg)](https://www.nuget.org/packages/Aliyun.Credentials)

## Installation

Use .NET CLI ( Recommand )

```sh
dotnet add package Aliyun.Credentials
```

## Quick Examples

Before you begin, you need to sign up for an Alibaba Cloud account and retrieve your [Credentials](https://usercenter.console.aliyun.com/#/manage/ak).

### Credential Type

#### AccessKey

Setup access_key credential through [User Information Management][ak], it have full authority over the account, please keep it safe. Sometimes for security reasons, you cannot hand over a primary account AccessKey with full access to the developer of a project. You may create a sub-account [RAM Sub-account][ram] , grant its [authorization][permissions]，and use the AccessKey of RAM Sub-account.

```csharp
using Aliyun.Credentials.Models;

namespace credentials_demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Config config = new Config()
            {
                // Which type of credential you want
                Type = "access_key",
                // AccessKeyId of your account
                AccessKeyId = "<AccessKeyId>",
                // AccessKeySecret of your account
                AccessKeySecret = "<AccessKeySecret>"
            };
            var client = new Aliyun.Credentials.Client(config);
            var credential = client.GetCredential();

            string accessKeyId = credential.AccessKeyId;
            string accessSecret = credential.AccessKeySecret;
            string credentialType = credential.Type;
        }
    }
}
```

#### STS

Create a temporary security credential by applying Temporary Security Credentials (TSC) through the Security Token Service (STS).

```csharp
using Aliyun.Credentials.Models;

namespace credentials_demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Config config = new Config()
            {
                // Which type of credential you want
                Type = "sts",
                // AccessKeyId of your account
                AccessKeyId = "<AccessKeyId>",
                // AccessKeySecret of your account
                AccessKeySecret = "<AccessKeySecret>",
                // Temporary Security Token
                SecurityToken = "<SecurityToken>"
            };
            var client = new Aliyun.Credentials.Client(config);
            var credential = client.GetCredential();

            string accessKeyId = credential.AccessKeyId;
            string accessSecret = credential.AccessKeySecret;
            string credentialType = credential.Type;
            string securityToken = credential.SecurityToken;
        }
    }
}
```

#### RamRoleArn

By specifying [RAM Role][RAM Role], the credential will be able to automatically request maintenance of STS Token. If you want to limit the permissions([How to make a policy][policy]) of STS Token, you can assign value for `Policy`.

```csharp
using Aliyun.Credentials.Models;

namespace credentials_demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Config config = new Config()
            {
                // Which type of credential you want
                Type = "ram_role_arn",
                // AccessKeyId of your account
                AccessKeyId = "<AccessKeyId>",
                // AccessKeySecret of your account
                AccessKeySecret = "<AccessKeySecret>",
                // Format: acs:ram::USER_Id:role/ROLE_NAME
                // RoleArn can be replaced by setting environment variable: ALIBABA_CLOUD_ROLE_ARN
                RoleArn = "<RoleArn>",
                // Role Session Name
                RoleSessionName = "<RoleSessionName>",
                // Optional, limit the permissions of STS Token
                Policy = "<Policy>",
                // Optional, limit the Valid time of STS Token
                RoleSessionExpiration = 3600,
            };
            var client = new Aliyun.Credentials.Client(config);
            var credential = client.GetCredential();

            string accessKeyId = credential.AccessKeyId;
            string accessSecret = credential.AccessKeySecret;
            string credentialType = credential.Type;
            string securityToken = credential.SecurityToken;
        }
    }
}
```

#### OIDCRoleArn

By specifying [OIDC Role][OIDC Role], the credential will be able to automatically request maintenance of STS Token. If you want to limit the permissions([How to make a policy][policy]) of STS Token, you can assign value for `Policy`.

```csharp
using Aliyun.Credentials.Models;

namespace credentials_demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Config config = new Config()
            {
                // Which type of credential you want
                Type = "oidc_role_arn",
                // Format: acs:ram::USER_Id:role/ROLE_NAME
                // RoleArn can be replaced by setting environment variable: ALIBABA_CLOUD_ROLE_ARN
                RoleArn = "<RoleArn>",
                // Format: acs:ram::USER_Id:oidc-provider/OIDC Providers 
                // OIDCProviderArn can be replaced by setting environment variable: ALIBABA_CLOUD_OIDC_PROVIDER_ARN  
                OIDCProviderArn = "<OIDCProviderArn>",
                // Format: path
                // OIDCTokenFilePath can be replaced by setting environment variable: ALIBABA_CLOUD_OIDC_TOKEN_FILE
                OIDCTokenFilePath = "/Users/xxx/xxx",
                // Role Session Name
                RoleSessionName = "<RoleSessionName>",
                // Optional, limit the permissions of STS Token
                Policy = "<Policy>,"
                // Optional, limit the Valid time of STS Token
                RoleSessionExpiration = 3600,

            };
            var client = new Aliyun.Credentials.Client(config);
            var credential = client.GetCredential();

            string accessKeyId = credential.AccessKeyId;
            string accessSecret = credential.AccessKeySecret;
            string credentialType = credential.Type;
            string securityToken = credential.SecurityToken;
        }
    }
}
```

#### EcsRamRole

By specifying the role name, the credential will be able to automatically request maintenance of STS Token.

```csharp
using Aliyun.Credentials.Models;

namespace credentials_demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Config config = new Config()
            {
                // Which type of credential you want
                Type = "ecs_ram_role",
                // Optional. It will be retrieved automatically if not set. It is highly recommended to set it up to reduce requests
                RoleName = "<RoleName>"
            };
            var client = new Aliyun.Credentials.Client(config);
            var credential = client.GetCredential();

            string accessKeyId = credential.AccessKeyId;
            string accessSecret = credential.AccessKeySecret;
            string credentialType = credential.Type;
            string securityToken = credential.SecurityToken;
        }
    }
}
```

#### URLCredential

By specifying the url, the credential will be able to automatically request maintenance of STS Token.

```csharp
using Aliyun.Credentials.Models;

namespace credentials_demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Config config = new Config()
            {
                // Which type of credential you want
                Type = "credentials_uri",
                // Format: http url
                // `CredentialsUri` can be replaced by setting environment variable: ALIBABA_CLOUD_CREDENTIALS_URI
                CredentialsUri = "http://xxx"
            };
            var client = new Aliyun.Credentials.Client(config);
            var credential = client.GetCredential();

            string accessKeyId = credential.AccessKeyId;
            string accessSecret = credential.AccessKeySecret;
            string credentialType = credential.Type;
            string securityToken = credential.SecurityToken;
        }
    }
}
```

#### Bearer Token

If credential is required by the Cloud Call Centre (CCC), please apply for Bearer Token maintenance by yourself.

```csharp
using Aliyun.Credentials.Models;

namespace credentials_demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Config config = new Config()
            {
                // Which type of credential you want
                Type = "bearer",        
                // BearerToken of your account
                BearerToken = "<BearerToken>"
            };
            var client = new Aliyun.Credentials.Client(config);
            var credential = client.GetCredential();

            string accessKeyId = credential.BearerToken;
            string credentialType = credential.Type;
        }
    }
}
```

### Provider

If you call `new Client(config)` with null, it will use provider chain to get credential for you.

#### 1. Environment Credentials

The program first looks for environment credentials in the environment variable. If the `ALICLOUD_ACCESS_KEY` and `ALICLOUD_SECRET_KEY` environment variables are defined and are not empty, the program will use them to create the default credential. If not, the program loads and looks for the client in the configuration file.

#### 2. Config File

If there is `~/.alibabacloud/credentials` default file (Windows shows `C:\Users\USER_NAME\.alibabacloud\credentials`), the program will automatically create credential with the name of 'default'. The default file may not exist, but a parse error throws an exception. The specified files can also be loaded indefinitely: `AlibabaCloud::load('/data/credentials', 'vfs://AlibabaCloud/credentials', ...);` This configuration file can be shared between different projects and between different tools. Because it is outside the project and will not be accidentally committed to the version control. Environment variables can be used on Windows to refer to the home directory %UserProfile%. Unix-like systems can use the environment variable $HOME or ~ (tilde). The path to the default file can be modified by defining the `ALIBABA_CLOUD_CREDENTIALS_FILE` environment variable.

```ini
[default]                          # Default credential
type = access_key                  # Certification type: access_key
access_key_id = foo                # access key id
access_key_secret = bar            # access key secret
```

#### 3. Instance RAM Role

If the environment variable `ALIBABA_CLOUD_ECS_METADATA` is defined and not empty, the program will take the value of the environment variable as the role name and request `http://100.100.100.200/latest/meta-data/ram/security-credentials/` to get the temporary Security credential.

## Issues

[Opening an Issue](https://github.com/aliyun/credentials-csharp/issues/new), Issues not conforming to the guidelines may be closed immediately.

## Changelog

Detailed changes for each release are documented in the [release notes](./ChangeLog.md).

## References

* [OpenAPI Developer Portal](https://next.api.aliyun.com/)
* [Latest Release](https://github.com/aliyun/credentials-csharp)

## License

[Apache-2.0](http://www.apache.org/licenses/LICENSE-2.0)

Copyright (c) 2009-present, Alibaba Cloud All rights reserved.

[ak]: https://usercenter.console.aliyun.com/#/manage/ak
[ram]: https://ram.console.aliyun.com/users
[policy]: https://www.alibabacloud.com/help/doc-detail/28664.htm?spm=a2c63.p38356.a3.3.27a63b01khWgdh
[permissions]: https://ram.console.aliyun.com/permissions
[RAM Role]: https://ram.console.aliyun.com/#/role/list
