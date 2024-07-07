---
title: Sign up
sidebar:
  order: 1
---

# Sign up

Classic email and password sign up endpoint. Triggers email confirmation if enabled.

| Info           |                                 |
|:---------------|:--------------------------------|
| Method         | `POST`                          |
| Route group    | `auth`                          |
| Path           | `{auth}/signup`                 |
| Type           | `SignUpEndpoint<TUser, TModel>` |
| Authentication | No                              |

### Request body*

| Name       | Type | Description |
|:-----------| :--- |:-|
| `Email`    | `string` |  |
| `Password` | `string` |  |

### Response body

| Name           | Type | Description |
|:---------------| :--- |:-|
| `RequiresEmailConfirmation`       | `boolean` | Information if email confirmation is required. |

*Request body can be modified to include additional fields. See [Sign up customizations](https://aufy.dev/docs/docs/signup-customizations) for more information.
