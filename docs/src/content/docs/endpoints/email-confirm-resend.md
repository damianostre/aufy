---
title: Resend Email Confirmation
sidebar:
  order: 7
---

# Resend email confirmation

| Info           |                                                |
|:---------------|:-----------------------------------------------|
| Method         | `POST`                                         |
| Route group    | `account`                                      |
| Path           | `{account}/email/confirm/resend`               |
| Type           | `EmailConfirmationResendEndpoint<TUser>`       |
| Authentication | No                                             |

### Request body

| Name    | Type     | Description   |
|:--------|:---------|:--------------|
| `Email` | `string` | User's email. |

