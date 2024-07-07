---
title: Token Refresh
sidebar:
  order: 5
---

# Refresh token

Cookie-less endpoint to obtain new access token and refresh token.

| Info           |                                  |
|:---------------|:---------------------------------|
| Method         | `POST`                           |
| Route group    | `auth`                           |
| Path           | `{auth}/token/refresh`           |
| Type           | `TokenRefreshEndpoint<TUser>`    |
| Authentication | Bearer token with refresh token. |

### Request body
-

### Response body

| Name            | Type     | Description                       |
|:----------------|:---------|:----------------------------------|
| `access_token`  | `string` | JWT `Bearer` token.               |
| `refresh_token` | `string` | JWT `Bearer` token.               |
| `expires_in`    | `number` | Token expiration time in seconds. |
| `token_type`    | `string` | `Bearer`                          |

