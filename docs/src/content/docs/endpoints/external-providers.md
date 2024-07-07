---
title: External Providers
sidebar:
  order: 11
---

List of external providers that can be used to authenticate users.

| Info           |                                    |
|:---------------|:-----------------------------------|
| Method         | `GET`                              |
| Route group    | `auth`                             |
| Path           | `{auth}/external/providers`        |
| Type           | `ExternalProvidersEndpoint<TUser>` |
| Authentication | No                                 |

### Response body

| Name        | Type       | Description       |
|:------------|:-----------|:------------------|
| `Providers` | `string[]` | List of providers |

