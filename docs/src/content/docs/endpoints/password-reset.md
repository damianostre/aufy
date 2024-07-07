---
title: Reset Password
sidebar:
  order: 9
---


| Info           |                                       |
|:---------------|:--------------------------------------|
| Method         | `POST`                                |
| Route group    | `account`                             |
| Path           | `{account}/password/reset`            |
| Type           | `PasswordResetEndpoint<TUser>`        |
| Authentication | No                                    |

### Request body

| Name       | Type     | Description                            |
|:-----------|:---------|:---------------------------------------|
| `Email`    | `string` | User's email.                          |
| `Code`     | `string` | Special code sent to the user's email. |
| `Password` | `string` | New password.                          |

