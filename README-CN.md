[English](./README.md) | 简体中文

![](https://aliyunsdk-pages.alicdn.com/icons/AlibabaCloud.svg)

## Alibaba Cloud Credentials for .NET

[![.NET CI](https://github.com/aliyun/credentials-csharp/actions/workflows/ci.yml/badge.svg)](https://github.com/aliyun/credentials-csharp/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/aliyun/credentials-csharp/graph/badge.svg?token=gZjatna6gL)](https://codecov.io/gh/aliyun/credentials-csharp)
[![Nuget Version](https://badge.fury.io/nu/Aliyun.Credentials.svg)](https://www.nuget.org/packages/Aliyun.Credentials)

## 安装

通过 .NET CLI 工具来安装

```sh
dotnet add package Aliyun.Credentials
```

## 快速使用

在您开始之前，您需要注册阿里云帐户并获取您的[凭证](https://usercenter.console.aliyun.com/#/manage/ak)。

### 凭证类型

#### AccessKey

通过[用户信息管理][ak]设置 access_key，它们具有该账户完全的权限，请妥善保管。有时出于安全考虑，您不能把具有完全访问权限的主账户 AccessKey 交于一个项目的开发者使用，您可以[创建RAM子账户][ram]并为子账户[授权][permissions]，使用RAM子用户的 AccessKey 来进行API调用。

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
                Type = "access_key",                    // 凭证类型
                AccessKeyId = "<AccessKeyId>",          // AccessKeyId
                AccessKeySecret = "<AccessKeySecret>"   // AccessKeySecret
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

通过安全令牌服务（Security Token Service，简称 STS），申请临时安全凭证（Temporary Security Credentials，简称 TSC），创建临时安全凭证。

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
                Type = "sts",                           // 凭证类型
                AccessKeyId = "<AccessKeyId>",          // AccessKeyId
                AccessKeySecret = "<AccessKeySecret>",  // AccessKeySecret
                SecurityToken = "<SecurityToken>"       // STS Token
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

通过指定[RAM角色][RAM Role]，让凭证自动申请维护 STS Token。你可以通过为 `Policy` 赋值来限制获取到的 STS Token 的权限。

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
                Type = "ram_role_arn",                  // 凭证类型
                AccessKeyId = "<AccessKeyId>",          // AccessKeyId
                AccessKeySecret = "<AccessKeySecret>",  // AccessKeySecret
                RoleArn = "<RoleArn>",                  // 格式: acs:ram::用户Id:role/角色名
                RoleSessionName = "<RoleSessionName>",  // 角色会话名称
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

通过指定角色名称，让凭证自动申请维护 STS Token

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
                Type = "ecs_ram_role",      // 凭证类型
                RoleName = "<RoleName>"     // 账户RoleName，非必填，不填则自动获取，建议设置，可以减少请求up to reduce requests
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

#### Bearer Token

如呼叫中心(CCC)需用此凭证，请自行申请维护 Bearer Token。

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
                Type = "bearer",                    // 凭证类型
                BearerToken = "<BearerToken>"       // BearerToken
            };
            var bearerCredential = new Aliyun.Credentials.Client(config);

            string bearerToken = bearerCredential.GetBearerToken();
            string credentialType = bearerCredential.GetType();
        }
    }
}
```

### 凭证提供程序链

如果你调用 `new Client(config)` 时传入 null， 将通过凭证提供链来为你获取凭证。

#### 1. 环境凭证

程序首先会在环境变量里寻找环境凭证，如果定义了 `ALICLOUD_ACCESS_KEY`  和 `ALICLOUD_SECRET_KEY` 环境变量且不为空，程序将使用他们创建凭证。如否则，程序会在配置文件中加载和寻找凭证。

#### 2. 配置文件

如果用户主目录存在默认文件 `~/.alibabacloud/credentials` （Windows 为 `C:\Users\USER_NAME\.alibabacloud\credentials`），程序会自动创建指定类型和名称的凭证。默认文件可以不存在，但解析错误会抛出异常。也可以手动加载指定文件： `AlibabaCloud::load('/data/credentials', 'vfs://AlibabaCloud/credentials', ...);` 不同的项目、工具之间可以共用这个配置文件，因为超出项目之外，也不会被意外提交到版本控制。Windows 上可以使用环境变量引用到主目录 %UserProfile%。类 Unix 的系统可以使用环境变量 $HOME 或 ~ (tilde)。 可以通过定义 `ALIBABA_CLOUD_CREDENTIALS_FILE` 环境变量修改默认文件的路径。

```ini
[default]                          # 默认凭证
type = access_key                  # 认证方式为 access_key
access_key_id = foo                # access key id
access_key_secret = bar            # access key secret
```

#### 3. 实例 RAM 角色

如果定义了环境变量 `ALIBABA_CLOUD_ECS_METADATA` 且不为空，程序会将该环境变量的值作为角色名称，请求 `http://100.100.100.200/latest/meta-data/ram/security-credentials/` 获取临时安全凭证。

## 问题

[提交 Issue](https://github.com/aliyun/credentials-csharp/issues/new)，不符合指南的问题可能会立即关闭。

## 发行说明

每个版本的详细更改记录在[发行说明](./ChangeLog.md)中。

## 相关

* [OpenAPI 开发者门户](https://next.api.aliyun.com/)
* [最新源码](https://github.com/aliyun/credentials-csharp)

## 许可证

[Apache-2.0](http://www.apache.org/licenses/LICENSE-2.0)

Copyright (c) 2009-present, Alibaba Cloud All rights reserved.

[ak]: https://usercenter.console.aliyun.com/#/manage/ak
[ram]: https://ram.console.aliyun.com/users
[permissions]: https://ram.console.aliyun.com/permissions
[RAM Role]: https://ram.console.aliyun.com/#/role/list
