---
title: Configuration
sidebar:
    order: 3
---

### Custom Identity user

Aufy come with a default user model `AufyUser`. If you want to use custom user model, you can inherit from `AufyUser` and add your own properties.

```csharp title="MyUser.cs"
public class MyUser : AufyUser
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
    .AddAufy<AufyUser>(builder.Configuration, options =>
    {
        options.AccountApiBasePath = "/my-account";
        options.AuthApiBasePath = "/my-auth";
    })
    ...
```

### Default User Role

By default, Aufy creates a new user without any roles.
You can change it by configuring `DefaultRoles` property in `AufyOptions`.

```csharp title="Program.cs"
builder.Services
    .AddAufy<AufyUser>(builder.Configuration, options =>
    {
        options.DefaultRoles = new[] { "MyRole" };
    })
    ...
```
