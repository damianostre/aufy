---
title: Sign in
sidebar:
  order: 2
---

Email and password sign in endpoint. Returns access token in the response body and refresh token as HTTP only cookie for better security.

| Info           |                         |
|:---------------|:------------------------|
| Method         | `POST`                  |
| Route group    | `auth`                  |
| Path           | `{auth}/signin`         |
| Type           | `SignInEndpoint<TUser>` |
| Authentication | No                      |

### Request body

| Name       | Type     | Description |
|:-----------|:---------|:------------|
| `Email`    | `string` |             |
| `Password` | `string` |             |

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
