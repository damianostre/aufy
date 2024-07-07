---
title: Token
sidebar:
  order: 3
---

Cookie-less endpoint to obtain access and refresh tokens with email and password.

| Info           |                        |
|:---------------|:-----------------------|
| Method         | `POST`                 |
| Route group    | `auth`                 |
| Path           | `{auth}/token`         |
| Type           | `TokenEndpoint<TUser>` |
| Authentication | No                     |

### Request body

| Name       | Type     | Description |
|:-----------|:---------|:------------|
| `Email`    | `string` |             |
| `Password` | `string` |             |

### Response body

| Name            | Type     | Description                       |
|:----------------|:---------|:----------------------------------|
| `access_token`  | `string` | JWT `Bearer` token.               |
| `refresh_token` | `string` | JWT `Bearer` token.               |
| `expires_in`    | `number` | Token expiration time in seconds. |
| `token_type`    | `string` | `Bearer`                          |

