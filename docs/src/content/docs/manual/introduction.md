---
title: Introduction
sidebar:
    order: 1
---

:::danger

Aufy is in beta stage. The library is under active development and may have breaking changes in the future. 
The documentation is a work in progress and may contain errors or incomplete information.

Use at your own risk.

:::


Aufy is a simple open-source .NET library for user authentication built on top of ASP.NET Core Identity.
It provides a set of Web API endpoints, helpers, and sample client applications.

## Features
- Ready to use API endpoints
- JWT bearer authentication with refresh tokens
- Social logins. Discord and GitHub configured in the templates
- Email/password authentication
- Email confirmation with resend of the confirmation email
- Password change and reset
- Sample Client applications
  - React
  - Vue
  - Angular
- Email sending abstraction for Email confirmation and password reset
- FluentEmail integration
  - Ready to use templates with customization options
- Sign Up endpoints customization
    - Custom sign up request model
- OpenAPI/Swagger support with customization options


## Roadmap to v1.0

- Stabilize the library and API
- Add one more client implementation (Vue, Angular, Blazor, etc.)
- Two-factor authentication with Authenticator app
- Improve documentation
