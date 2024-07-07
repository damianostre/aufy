---
title:  Social logins / OAuth
sidebar:
  order: 6
---

## Configuration

Providers configuration is located in `Aufy:Providers` section of your appsettings file.
Aufy provides pre-configured authentication schemes for `GitHub` and `Discord`.

```json title="appsettings.json"
{
  "Aufy": {
    "Providers": {
      "GitHub": {
        "ClientId": "MY_CLIENT_ID",
        "ClientSecret": "MY_CLIENT_SECRET",
        "Scopes": [
          "user:email",
          "read:user"
        ]
      },
      "Discord": {
        "ClientId": "MY_CLIENT_ID",
        "ClientSecret": "MY_CLIENT_SECRET",
        "Scopes": [
          "email"
        ]
      }
    }
  }
}
```

### Configuring OAuth providers

To configure OAuth provider using Aufy helpers, add a new section to `Aufy:Providers` section of your appsettings file.

```json title="appsettings.json"
{
  "Aufy": {
    "Providers": {
      "Google": {
        "ClientId": "MY_CLIENT_ID",
        "ClientSecret": "MY_CLIENT_SECRET",
        "Scopes": [
          "email"
        ]
      }
    }
  }
}
```

Next register OAuth Scheme using Aufy helpers.

```csharp title="Program.cs"
builder.Services
    .AddAufy<AufyUser>(builder.Configuration)
    .AddEntityFrameworkStore<ApplicationDbContext>()
    .AddProvider(DiscordAuthenticationDefaults.AuthenticationScheme, (auth, options) =>
    {
        auth.AddDiscord(o => o.Configure(DiscordAuthenticationDefaults.AuthenticationScheme, options));
    });
```
* `AddProvider` method will execute only if section with specified name exists in `Aufy:Providers` section of your appsettings file.
* `Configure` method will apply specified options from `Aufy:Providers` section of your appsettings file and some defaults required by Aufy:
  * `ClientId` and `ClientSecret` options are required.
  * `Scopes`
  * `CallbackPath` in `{ApiBasePath}/external/callback/{Provider}` format. For example: `/auth/external/callback/google`. 
     You have to configure this callback path in your OAuth provider dashboard.
  * Cookie `SingInScheme` used later for final sign in/sign up. The value is `Aufy.ExternalSignInDefaultScheme`.
  * OAuth.Events.OnCreatingTicket set to internal Aufy handler that checks if user exists. If user exist it creates special sign in cookies. Otherwise, it creates a special sign up cookie and adds query parameter `signup=true` to the callback URL.
