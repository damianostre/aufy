---
title: JWT Bearer Authentication
sidebar:
    order: 5
---

Aufy uses JWT bearer tokens for authentication. Aufy reads bearer token from `Authorization` header in the following format:
```
Authorization: Bearer <token
```

Refresh tokens can work in two ways:
- Refresh token is sent in a cookie with `HttpOnly` flag set to `true`. (`/signin` endpoints)
- Refresh token is sent in a response body - for cookieless environments. (`/token` endpoints)

# JWT Token Configuration

## Configuration

Add `JwtBearer` subsection to `Aufy` configuration section in your appsettings file.

```json title="appsettings.json"
{
  "Aufy": {
    "JwtBearer": {
      "SigningKey": "super secret key",
      "Issuer": "MY_ISSUER",
      "Audience": "MY_AUDIENCE",
      "AccessTokenExpiresInMinutes": 5,
      "RefreshTokenExpiresInHours": 48
    }
  }
}
```

:::info

Default value for `AccessTokenExpiresInMinutes` is `5` and for `RefreshTokenExpiresInHours` is `48`.

:::

## Refresh token

* Refresh token is a special token that can be used to obtain a new access token.
* Refresh tokens are stored in database.
* Refresh tokens are returned from API opaqued in JWT token. That token is used to obtain a new access token.
* Refresh tokens are valid for 48 hours by default.
* Refresh tokens are invalidated when user changes password or signs out.
* New refresh token is generated every time user signs in and refresh token is used to obtain a new access token.
