---
title: Change Password
sidebar:
  order: 10
---

| Info           |                                 |
|:---------------|:--------------------------------|
| Method         | `POST`                          |
| Route group    | `account`                       |
| Path           | `{account}/password/change`     |
| Type           | `PasswordChangeEndpoint<TUser>` |
| Authentication | C                               |

### Request body

| Name          | Type     | Description       |
|:--------------|:---------|:------------------|
| `Password`    | `string` | Current password. |
| `NewPassword` | `string` | New password.     |

