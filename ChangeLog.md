### 2024-09-14 Version 1.4.2
* feat: support cli credentials provider 
* refactor: get metadata token every time refresh credentials

### 2024-09-05 Version 1.4.1
* feat: support AssumeRoleWithOIDC
* feat: support URLCredential
* feat: add user-agnet for all credential requests
* feat: support env ALIBABA_CLOUD_SECURITY_TOKEN 
* feat: support internal static_ak/static_sts credentials provider
* fix: solve refresh issue in EcsRamRoleCredentialProvider
* fix: clean useless codes

### 2024-07-24 Version 1.4.0
* refactor: solve the inconsistency of credentials refresh
* fix: capitalize methodType and pass through response info when catch webException

### 2023-05-04 Version 1.3.3
* Fix default type for credentials
* Bump Newtonsoft.Json from 9.0.1 to 13.0.1 in /aliyun-net-credentials

### 2022-12-05 Version 1.3.2
* Fix refresh failure caused by time zone and slot

### 2020-08-14 Version 1.3.1
* Replenish Refresh Credential
* Add `BaseCredential`

### 2020-02-24 Version 1.2.1
* Improved Config

### 2020-02-17 Version 1.1.1
* Supported Async

### 2020-02-11 Version 1.0.1
* Improved Construct

### 2020-01-10 Version 0.0.3
* Fixed const type name

### 2020-01-01 Version 0.0.2
* Supported auto set RoleName

### 2019-10-21 Version 0.0.1
* first release