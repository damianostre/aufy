---
title: Endpoints customization
sidebar:
    order: 8
---

# Endpoints customization

Aufy allows to customize the behavior of the endpoints by exposing `RouteHandlerBuilder` in the `AddAufy` method. 

```csharp title="Program.cs"
app.MapAufyEndpoints(c =>
{
    c.ConfigureRoute<SignInEndpoint<AufyUser>>(routeHandlerBuilder => { });
});
```
`ConfigureRoute` requires generic parameter `T` which is the type of the endpoint to be configured. 
List of available endpoints can be found in the [Endpoints](

# Route group customization

All endpoints are grouped under a single route group, which can be customized using `ConfigureAuthRouteGroup` method.

```csharp title="Program.cs"
app.MapAufyEndpoints(c =>
{
    c.ConfigureAuthRouteGroup(routeGroupBuilder => {});
});
```
