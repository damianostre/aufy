---
title: Sending emails
sidebar:
    order: 7
---

* Aufy core library provides email sending abstraction in form of interfaces for each type of email.
  * IAufyEmailConfirmationEmailSender - sends email with confirmation link.
  * IAufyPasswordResetEmailSender - sends email with password reset link.
* Aufy comes with FluentEmail library out of the box, but you can easily replace it with your own implementation.
* FluentEmail comes in a separate package `Aufy.FluentEmail`.

## Configuration

Install `Aufy.FluentEmail` nuget package and register FluentEmail services using `AddFluentEmail` extension method.

```csharp title="Program.cs"
builder.Services
    .AddAufy<AufyUser>(builder.Configuration)
    .AddEntityFrameworkStore<ApplicationDbContext>()
    .AddFluentEmail();
```
If you already use FluentEmail in your project, you need to pass `false` as a parameter to `AddEmailSender` method.
This will skip registration of `ISender` service and use your existing registration.

```csharp title="Program.cs"
builder.Services
    .AddAufy<AUfyUser>(builder.Configuration)
    .AddEntityFrameworkStore<ApplicationDbContext>()
    .AddFluentEmail(registerMailKitSender: false);
```

By default, FluentEmail integration will try to read `FluentEmail` section from your appsettings file.
Optionally, you can configure `FluentEmailOptions` using `Configure<FluentEmailOptions>` method.

```csharp title="Program.cs"
builder.Services.Configure<FluentEmailOptions>(/* configure options */);
``` 
* If you already use FluentEmail in your project you can skip configuration of properties with `Smtp` prefix.

```json title="appsettings.json"
{
    "FluentEmail": {
      "SmtpHost": "[YOUR_HOST]",
      "SmtpPort": 1025,
      "SmtpUsername": "[YOUR_USERNAME]",
      "SmtpPassword": "[YOUR_`PASSWORD]",
      
      "FromEmail": "from_your_email@host.host",
      "FromName": "Your Sender Name"
    }
}
```

## Templates

* Fluent Email integration uses  [Fluid](https://github.com/sebastienros/fluid) template engine to render email templates, 
which is based on Liquid markup language.
* FluentEmail integration comes with default templates that are embedded in the package. 
Default templates can be overridden by providing your own templates in the `FluentEmail` section of your appsettings file.
* There is also a possibility to provide custom email subject.

```json title="appsettings.json"
{
    "FluentEmail": {
      "Emails":{
        "Confirmation": {
          "TemplatePath": "PATH_TO_TEMPLATE/Confirmation.html",
          "Subject": "Just confirm!!!"
        }
      }
    }
}
```
Those options can be configured using `FluentEmailOptions` configuration.

```csharp title="Program.cs"
builder.Services.Configure<FluentEmailOptions>(/* configure options */);
```




