---
title: Sign out
sidebar:
  order: 15
---

Sign out operation invalidates the refresh token in the database and removes the refresh token cookie from the client.

| Info           |                          |
|:---------------|:-------------------------|
| Method         | `POST`                   |
| Route group    | `auth`                   |
| Path           | `{auth}/signout`         |
| Type           | `SignOutEndpoint<TUser>` |
| Authentication | Bearer                   |
