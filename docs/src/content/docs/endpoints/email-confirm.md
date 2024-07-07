---
title: Confirm Email
sidebar:
  order: 6
---

# Confirm email

| Info           |                                        |
|:---------------|:---------------------------------------|
| Method         | `GET`                                  |
| Route group    | `account`                              |
| Path           | `{account}/email/confirm`              |
| Type           | `EmailConfirmEndpoint<TUser>`          |
| Authentication | No                                     |

### Query parameters

| Name     | Type     | Description                            |
|:---------|:---------|:---------------------------------------|
| `code`   | `string` | Special code sent to the user's email. |
| `userId` | `string` | User's unique identifier.              |

