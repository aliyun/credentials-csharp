English | [简体中文](./README-CN.md)

![](https://aliyunsdk-pages.alicdn.com/icons/AlibabaCloud.svg)

## Alibaba Cloud Credentials for .NET

[![Travis Build Status](https://travis-ci.org/aliyun/credentials-csharp.svg?branch=master)](https://travis-ci.org/aliyun/credentials-csharp)
[![codecov](https://codecov.io/gh/aliyun/credentials-csharp/branch/master/graph/badge.svg)](https://codecov.io/gh/aliyun/credentials-csharp)

## Installation

Use .NET CLI ( Recommand )

    dotnet add package Aliyun.Credentials

Use Package Manager

    Install-Package Aliyun.Credentials

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
                Type = "access_key",                    // Which type of credential you want
                AccessKeyId = "<AccessKeyId>",          // AccessKeyId of your account
                AccessKeySecret = "<AccessKeySecret>"   // AccessKeySecret of your account
            };
            var akCredential = new Aliyun.Credentials.Client(config);

            string accessKeyId = akCredential.GetAccessKeyId();
            string accessSecret = akCredential.GetAccessKeySecret();
            string credentialType = akCredential.GetType();
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
                Type = "sts",                           // Which type of credential you want
                AccessKeyId = "<AccessKeyId>",          // AccessKeyId of your account
                AccessKeySecret = "<AccessKeySecret>",  // AccessKeySecret of your account
                SecurityToken = "<SecurityToken>"       // Temporary Security Token
            };
            var stsCredential = new Aliyun.Credentials.Client(config);

            string accessKeyId = stsCredential.GetAccessKeyId();
            string accessSecret = stsCredential.GetAccessKeySecret();
            string credentialType = stsCredential.GetType();
            string securityToken = stsCredential.GetSecurityToken();
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
                Type = "ram_role_arn",                  // Which type of credential you want
                AccessKeyId = "<AccessKeyId>",          // AccessKeyId of your account
                AccessKeySecret = "<AccessKeySecret>",  // AccessKeySecret of your account
                RoleArn = "<RoleArn>",                  // Format: acs:ram::USER_Id:role/ROLE_NAME
                RoleSessionName = "<RoleSessionName>",  // Role Session Name
            };
            var arnCredential = new Aliyun.Credentials.Client(config);

            string accessKeyId = arnCredential.GetAccessKeyId();
            string accessSecret = arnCredential.GetAccessKeySecret();
            string credentialType = arnCredential.GetType();
            string securityToken = arnCredential.GetSecurityToken();
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
                Type = "ecs_ram_role",      // Which type of credential you want
                RoleName = "<RoleName>"     // `roleName` is optional. It will be retrieved automatically if not set. It is highly recommended to set it up to reduce requests
            };
            var ecsCredential = new Aliyun.Credentials.Client(config);

            string accessKeyId = ecsCredential.GetAccessKeyId();
            string accessSecret = ecsCredential.GetAccessKeySecret();
            string credentialType = ecsCredential.GetType();
            string securityToken = ecsCredential.GetSecurityToken();
        }
    }
}
```

#### RsaKeyPair
By specifying the public key Id and the private key file, the credential will be able to automatically request maintenance of the AccessKey before sending the request. Only Japan station is supported. 
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
                Type = "rsa_key_pair",                  // Which type of credential you want
                PrivateKeyFile = "<PrivateKeyFile>",    // The file path to store the PrivateKey
                PublicKeyId = "<PublicKeyId>"           // PublicKeyId of your account
            };
            var rsaCredential = new Aliyun.Credentials.Client(config);

            string accessKeyId = rsaCredential.GetAccessKeyId();
            string accessSecret = rsaCredential.GetAccessKeySecret();
            string credentialType = rsaCredential.GetType();
            string securityToken = rsaCredential.GetSecurityToken();
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
                Type = "bearer",                    // Which type of credential you want
                BearerToken = "<BearerToken>"       // BearerToken of your account
            };
            var bearerCredential = new Aliyun.Credentials.Client(config);

            string bearerToken = bearerCredential.GetBearerToken();
            string credentialType = bearerCredential.GetType();
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
* [OpenAPI Explorer](https://api.aliyun.com/)
* [Latest Release](https://github.com/aliyun/credentials-csharp)

## License
[Apache-2.0](http://www.apache.org/licenses/LICENSE-2.0)

Copyright (c) 2009-present, Alibaba Cloud All rights reserved.

[ak]: https://usercenter.console.aliyun.com/#/manage/ak
[ram]: https://ram.console.aliyun.com/users
[policy]: https://www.alibabacloud.com/help/doc-detail/28664.htm?spm=a2c63.p38356.a3.3.27a63b01khWgdh
[permissions]: https://ram.console.aliyun.com/permissions
[RAM Role]: https://ram.console.aliyun.com/#/role/list