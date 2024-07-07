---
title: Forgot Password
sidebar:
  order: 8
---

# Forgot password

Forgot password endpoint. Sends email with reset password link to user's email.

| Info           |                                       |
|:---------------|:--------------------------------------|
| Method         | `POST`                                |
| Route group    | `account`                             |
| Path           | `{account}/password/forgot`           |
| Type           | `PasswordForgotEndpoint<TUser>`       |
| Authentication | No                                    |

### Request body

| Name    | Type     | Description   |
|:--------|:---------|:--------------|
| `Email` | `string` | User's email. |

