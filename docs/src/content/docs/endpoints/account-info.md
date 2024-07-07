---
title: Account Info
sidebar:
  order: 17
---

Base information about the account.

| Info           |                              |
|:---------------|:-----------------------------|
| Method         | `GET`                        |
| Route group    | `account`                    |
| Path           | `{account}/info`             |
| Type           | `AccountInfoEndpoint<TUser>` |
| Authentication | Bearer                       |

### Response body

| Name          | Type       | Description |
|:--------------|:-----------|:------------|
| `Email`       | `string`   |             |
| `Username`    | `string`   |             |
| `Roles`       | `string[]` |             |
| `Logins`      | `string[]` |             |
| `HasPassword` | `boolean`  |             |


