---
title: Sign in refresh
sidebar:
  order: 4
---

# Sign in refresh

Refresh token endpoint with cookie-based authentication.

| Info          |                                      |
|:--------------|:-------------------------------------|
| Method        | `POST`                               |
| Route group   | `auth`                               |
| Path          | `{auth}/signin/refresh`              |
| Type          | `SignInRefreshEndpoint<TUser>`       |
| Authenticated | Http only cookie with refresh token. |

### Request body
-

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

| Name                | Type     | Description                                                                                  |
|:--------------------|:---------|:---------------------------------------------------------------------------------------------|
| `Aufy.RefreshToken` | `string` | HTTP only cookie with refresh token.                                                         |
| `Aufy.AccessToken`  | `string` | HTTP only cookie with access token. Set only if `usecookie` query parameter is `true`.       |
