---
title: Installation
sidebar:
    order: 2
---

:::danger

Aufy is in beta stage. The library is under active development and may have breaking changes in the future.
The documentation is a work in progress and may contain errors or incomplete information.

Use at your own risk.

:::

:::caution

The recommended way to get started with Aufy is to use the one of the provided examples.

See [Aufy starters](./../../starters/readme) for more information.
:::

Aufy is built on top of ASP.NET Core Identity framework, which is database agnostic.
Currently only Entity Framework Core implementation is available, but you can use any other database provider.

### Install nuget package

```bash
dotnet add package Aufy.EntityFrameworkCore
```

### Create DbContext

Add a new DbContext class that inherits from `AufyDbContext`.

```csharp title="MyAuthDbContext.cs"
public class MyAuthDbContext : AufyDbContext<AufyUser>
{
    public MyAuthDbContext(DbContextOptions<MyAuthDbContext> options) : base(options)
    {
    }
}
```

### Register your DbContext in DI container

Sample registration with SQLite database:

```json title="appsettings.json"
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=auth.db"
  }
}
```

```csharp title="Program.cs"
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")

builder.Services.AddDbContext<MyAuthDbContext>(options => options.UseSqlite(connectionString));
```

### Register Aufy services in DI container

```csharp title="Program.cs"
builder.Services
    .AddAufy<AufyUser>(builder.Configuration, opts => { /* configure options here */ })
    .AddEntityFrameworkStore<MyAuthDbContext, AufyUser>()
```

### Add JWT Bearer configuration

```json title="appsettings.json"
{
  "Aufy": {
    "JwtBearer": {
      "SigningKey": "super secret key",
      "Issuer": "MY_ISSUER",
      "Audience": "MY_AUDIENCE"
    }
  }
}
```

### Add authentication and authorization middleware and Aufy endpoints

```csharp title="Program.cs"
app.UseAuthentication();
app.UseAuthorization();

app.MapAufyEndpoints();
```

### Add migrations and update database

```bash
dotnet ef migrations add MIGRATION_NAME
dotnet ef database update
```

### Congratulations! You have successfully installed Aufy!
