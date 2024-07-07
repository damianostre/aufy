---
title: Who am I
sidebar:
  order: 16
---

The endpoint to retrieve the current user's information from JWT token.

| Info           |                         |
|:---------------|:------------------------|
| Method         | `GET`                   |
| Route group    | `auth`                  |
| Path           | `{auth}/whoami`         |
| Type           | `WhoAmIEndpoint<TUser>` |
| Authentication | Bearer                  |

### Response body

| Name       | Type       | Description |
|:-----------|:-----------|:------------|
| `Username` | `string`   |             |
| `Email`    | `string`   |             |
| `Roles`    | `string[]` |             |
