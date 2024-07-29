---
title: Configuration
sidebar:
    order: 3
---

### Identity user

Aufy requires an Identity User class that extends `IdentityUser` and implements `IAufyUser` interface. For simplicity and some security reasons other than IdentityUser<string> (same as IdentityUser) are not supported.

```csharp title="MyUser.cs"

Example user class:

```csharp title="MyUser.cs"
public class MyUser: IdentityUser, IAufyUser
{
    public string MyProperty { get; set; }
}
```

### Custom api path

By default, Aufy endpoints are available under `api/auth` for  authentication and `api/account` for account management.
You can change it by configuring `AuthApiBasePath` and `AccountApiBasePath` properties in `AufyOptions`. 
The best option to do it is to add an optional parameter to `AddAufy` function 

```csharp title="Program.cs"
builder.Services
    .AddAufy<MyUser>(builder.Configuration, options =>
    {
        options.AccountApiBasePath = "/my-account";
        options.AuthApiBasePath = "/my-auth";
    })
    ...
```
### Automatic Account Linking

By default, Aufy tries to automatically link the user account with the external provider account if the email address is the same. 
This allows the user to sign in with the same account regardless of the authentication method used. 
Although this is a convenient feature, there are some security concerns, and it may not be suitable for all applications.

If you want to disable this feature, you can set `AutoAccountLinking` to `false`.

```csharp title="Program.cs"
builder.Services
    .AddAufy<MyUser>(builder.Configuration, options =>
    {
        options.AutoAccountLinking = false;
    })
    ...
```


### Default User Role

By default, Aufy creates a new user without any roles.
You can change it by configuring `DefaultRoles` property in `AufyOptions`.

```csharp title="Program.cs"
builder.Services
    .AddAufy<MyUser>(builder.Configuration, options =>
    {
        options.DefaultRoles = new[] { "MyRole" };
    })
    ...
```
