---
title:  Social logins / OAuth
sidebar:
  order: 6
---

# Configuration

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

## Configuring OAuth providers

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

# Sign In / Sign Up flow

1. Redirect user to `[Auth prefix]/external/challenge/[Provider name]` endpoint.
   * Example: `/auth/external/challenge/discord`
   * Pass `callbackUrl` query parameter with URL to redirect after challenge.
2. User will be redirected to OAuth provider login page.
3. Regardless of the authentication result, user will be redirected to the `callbackUrl`, optionally with additional query parameters:
   * If login was successful no additional query parameters will be added and external auth cookie will be set.
   * If login was unsuccessful `failed=true` query parameter will be added to the callback URL.
   * If custom sign up flow is enabled and user doesn't have an account `signup=true` query parameter will be added to the callback URL.
4. If external login was successful call:
   * When no query parameters are present `[Auth prefix]/signin/external` 
   * When `signup=true` query parameter is present `[Auth prefix]/signup/external`
5. Either sign in or sign up endpoint will return access token in the response body and refresh token as HTTP only cookie.

# Link login flow

Aufy by default tries to link external provider account with the existing user account if the email address is the same.
The other way to link existing user account with external login is to use `[Auth prefix]/link/external` endpoint. 

1. User must be authenticated and have valid access token.
2. Redirect user to `[Auth prefix]/external/challenge/[Provider name]` endpoint.
    * Example: `/auth/external/challenge/discord`
    * Pass `callbackUrl` query parameter with URL to redirect after challenge.
3. User will be redirected to OAuth provider login page.
4. Regardless of the authentication result, user will be redirected to the `callbackUrl`, optionally with additional query parameters:
    * If login was successful no additional query parameters will be added and external auth cookie will be set.
    * If login was unsuccessful `failed=true` query parameter will be added to the callback URL.
5. If external login was successful call `[Account prefix]/link/external` endpoint.
6. Link endpoint return account information in the response body, including updated list of external logins assigned to the user.