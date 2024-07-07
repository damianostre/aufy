---
title: FAQ
---

## What is Aufy?

Aufy is a simple open-source .NET library for user authentication built on top of ASP.NET Core Identity.
It provides a set of Web API endpoints, helpers, and sample client applications.

## Why did you create Aufy?

Aufy started as an authentication solution for my personal project. It was before Identity API endpoints were even announced (yeah, I know it took me a while).
I realized there was no simple and ready to use solution for authentication that I could use, so I decided to create one.
Introduction of Identity API endpoints did not change my mind, as I wanted to have more control over the library and its features.

## What Sign-In methods does Aufy support?

Aufy supports email/password and social logins (Discord and GitHub preconfigured in sample client applications).

## What authentication methods does Aufy support?

Aufy supports only JWT bearer tokens with refresh tokens.

## Does Aufy support cookie authentication?

Aufy supports storing JWT bearer tokens in HttpOnly cookies, but it does not support classic ASP.NET Core Identity cookie authentication.

## Does Aufy supports OAuth?

Aufy supports OAuth as a consumer only. It does not provide OAuth endpoints.

## What is a tech stack?

Aufy is built with the following technologies:

- .NET 8 and ASP.NET Core
- Minimal API for Web API endpoints
- Integrations:
  - Entity Framework Core
  - FluentEmail
- Client applications are built with:
  - React
  - More to come...

## Does it use the Identity API endpoints under the hood?

No, Aufy is built on top of ASP.NET Core Identity framework, but it does not use Identity API endpoints.

## What is the difference between Aufy and Identity API endpoints?

Both Aufy and Identity API endpoints are built on top of ASP.NET Core Identity and allow to easily add authentication to your application. 
The comparison between the two is as follows:

| Feature                            | Aufy         | Identity API endpoints |
|:-----------------------------------|:-------------|:-----------------------|
| Built on top of Identity framework | 🟢           | 🟢                     |
| Web API included                   | 🟢           | 🟢                     |
| Bearer authentication              | 🟢           | 🟢                     |
| JWT Tokens                         | 🟢           | 🔴                     |
| Refresh tokens                     | 🟢           | 🟢                     |
| Cookie authentication              | 🟢           | 🟢                     |
| Email and password authentication  | 🟢           | 🟢                     |
| Social logins                      | 🟢           | 🔴                     |
| Two-factor authentication          | 🟡 (Planned) | 🟢                     |
| Email confirmation                 | 🟢           | 🟢                     |
| Password reset                     | 🟢           | 🟢                     |
| Password change                    | 🟢           | 🟢                     |
| Email change                       | 🔴           | 🟢                     |
| Emails sending                     | 🟢           | 🔴 (Only abstracted)   |
| Customizable                       | 🟢           | 🔴                     |
| Ready to use client applications   | 🟢           | 🔴                     |
| Future improvements                | 🟢           | 🟡 (Hard to say)       |
| Community impact on development    | 🟢           | 🟡 (Limited)           |


