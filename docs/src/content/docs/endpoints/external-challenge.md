---
title: External Challenge
sidebar:
  order: 12
---

List of external providers that can be used to authenticate users.

| Info           |                                       |
|:---------------|:--------------------------------------|
| Method         | `GET`                                 |
| Route group    | `auth`                                |
| Path           | `{auth}/external/challenge/:provider` |
| Type           | `ExternalChallengeEndpoint<TUser>`    |
| Authentication | No                                    |

### Path parameters

| Name       | Type     | Description       |
|:-----------|:---------|:------------------|
| `provider` | `string` | Provider's name.  |

### Query parameters

| Name          | Type     | Description                      |
|:--------------|:---------|:---------------------------------|
| `callbackUrl` | `string` | URL to redirect after challenge. |


:::info

Whether the user has already an account the response will be different.

:::

If user doesn't have an account, the response will be:

### Response cookies

| Name                        | Type     | Description |
|:----------------------------|:---------|:------------|
| `Aufy.ExternalSignUpScheme` | HttpOnly |             |

### Callback URL query parameters

* signup=true - useful for the frontend to know that the user is signing up.

If user has an account, the response will be:

### Response cookies

| Name                        | Type     | Description |
|:----------------------------|:---------|:------------|
| `Aufy.ExternalSignInScheme` | HttpOnly |             |


