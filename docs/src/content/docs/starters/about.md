---
title: About
sidebar:
    order: 2
---

Starter application templates is the easiest way to start with Aufy. It provides a ready to use application with Aufy library and client application.

## Starter template structure

Each starter template application consists of a .NET solution (`WebApp.[Starter Type].sln`) with the following projects:
- Backend Web API project with Aufy library (`WebApi.[Starter Type]`)
- Frontend client application (`Client.[Starter Type]`)
- Shared typescript project with API client and models (`AufyClient`)

Each starter template shares the same Web API codebase, but have separate projects. The client application is different for each starter template.

## WebApi project

Each `WebApi` project has the following features:
- `Aufy.EntityFrameworkCore` integration with pre-configured `Sqlite` database
- `Aufy.FluntEmail` integration with pre-configured email templates.
  - In `Development` environment, emails are saved to the file system
- Serilog logging with pre-configured console and file sinks
- OpenAPI/Swagger
- Discord and GitHub social logins pre-configured

## AufyClient project

`AufyClient` provides:
- Shared API client
- Default token and user storage implementation with `Local Storage` and `HttpOnly` cookies
- Shared models
- Axios HTTP client with pre-configured interceptors for refresh tokens and error handling

`AufyClient` can be used as a standalone library in any other project.

## Client project

Each `Client` project implements the JWT Bearer token authentication flow with short-lived access tokens stored in Local Storage and long-lived refresh tokens stored in HttpOnly cookies.

Features of starter templates:
- Email and password sign-up
- Email and password sign-in
- Email verification
- Email verification resend
- Password reset
- Password change
- Social logins (Discord and GitHub)
- Refresh token flow
- My account page
- Sign out

## Custom Sign up flow

Starters have ready to use example of custom sign up flow that allows to send additional data with sign up request.

To enable custom sign up flow for `WebApi` project, you need to:

* Find `ServicesExtensions.cs` file in `WebApi` project
* Find `SetupAufy` function which configures Aufy services.
* Add execution of `.UseAufyCustomSignup()` method to the existing chain of methods.

```csharp
services
    .AddAufy<MyUser>(configuration)
    ...
    ...
    .UseAufyCustomSignup(); // Add this line
```
 There should be already a commented out example of custom sign up flow in `ServicesExtensions.cs` file.

:::caution

To enable custom sign up flow for `Client` project, go to corresponding starter documentation. 

:::



