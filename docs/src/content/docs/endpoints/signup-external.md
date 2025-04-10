---
title: Sign up with external provider
sidebar:
  order: 14
---

:::caution

Sign up with external provider endpoint is disabled by default. 
Basic oauth flow is covered by the [Sign in with external provider](/endpoints/signin-external) endpoint and should be enough for most applications.

If user sign up requires additional data from the user, you can use this endpoint to collect it.
See below for more information.

:::


To use this endpoint, you need to configure external sign up model by calling `UseExternalSignUpModel<TModel>` method:

```csharp title="Program.cs"
builder.Services
    .AddAufy<MyUser>(builder.Configuration)
    .UseExternalSignUpModel<MySignUpExternalRequest>();
    ...
```

```csharp title="MySignUpExternalRequest.cs"
public class MySignUpExternalRequest
{
    public string AboutMe { get; set; }
    public string MySiteUrl { get; set; }
}
```
This will enable the endpoint and use the model as a request body.

Sign up with external provider endpoint using a special cookie generated by a challenge endpoint.
Returns access token in the response body and refresh token as HTTP only cookie for better security.

| Info           |                                         |
|:---------------|:----------------------------------------|
| Method         | `POST`                                  |
| Route group    | `auth`                                  |
| Path           | `{auth}/signup/external`                |
| Type           | `SignUpExternalEndpoint<TUser, TModel>` |
| Authentication | `Aufy.ExternalSignUpScheme` cookie      |

### Request body

Custom request body model defined by `UseExternalSignUpModel<TModel>` method.

| Name | Type | Description |
|:-----|:-----|:------------|
| -    |      |             |

### Query parameters

| Name        | Type      | Description                                           |
|:------------|:----------|:------------------------------------------------------|
| `usecookie` | `boolean` | Use cookie to store access token. Default is `false`. |

### Response body

| Name           | Type     | Description                       |
|:---------------|:---------|:----------------------------------|
| `access_token` | `string` | JWT token.                        |
| `expires_in`   | `number` | Token expiration time in seconds. |
| `token_type`   | `string` | `Bearer`                          |

### Response Cookies

| Name                | Type     | Description                                                                            |
|:--------------------|:---------|:---------------------------------------------------------------------------------------|
| `Aufy.RefreshToken` | `string` | HTTP only cookie with refresh token.                                                   |
| `Aufy.AccessToken`  | `string` | HTTP only cookie with access token. Set only if `usecookie` query parameter is `true`. |
