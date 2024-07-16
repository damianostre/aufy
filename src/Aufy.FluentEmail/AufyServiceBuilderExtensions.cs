using Aufy.Core;
using Aufy.Core.EmailSender;
using FluentEmail.Core.Defaults;
using FluentEmail.Core.Interfaces;
using FluentEmail.MailKitSmtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Aufy.FluentEmail;

public static class AufyServiceBuilderExtensions
{
    public static AufyServiceBuilder<TUser> AddFluentEmail<TUser>(
        this AufyServiceBuilder<TUser> builder,
        bool registerMailKitSender = true) where TUser : IdentityUser, IAufyUser, new()
    {
        if (builder.Configuration.GetSection(FluentEmailOptions.SectionName).Exists())
        {
            builder.Services.Configure<FluentEmailOptions>(
                builder.Configuration.GetSection(FluentEmailOptions.SectionName));
        }
        else
        {
            builder.Services.Configure<FluentEmailOptions>(_ => { });
        }

        var saveEmailsOnDisk = builder.Configuration.GetValue<string?>($"{FluentEmailOptions.SectionName}:SaveEmailsOnDisk");
        if (saveEmailsOnDisk is not null)
        {
            Directory.CreateDirectory(saveEmailsOnDisk);
            builder.Services.TryAdd(ServiceDescriptor.Scoped<ISender>(_ => new SaveToDiskSender(saveEmailsOnDisk)));
        }
        else if (registerMailKitSender)
        {
            builder.Services.TryAdd(ServiceDescriptor.Scoped<ISender>(
                sp =>
                {
                    var opts = sp.GetRequiredService<IOptions<FluentEmailOptions>>();
                    var smtpClientOptions = new SmtpClientOptions
                    {
                        Server = opts.Value.SmtpHost,
                        Port = opts.Value.SmtpPort,
                        User = opts.Value.SmtpUsername,
                        Password = opts.Value.SmtpPassword,
                    };
                    return new MailKitSender(smtpClientOptions);
                }));
        }

        builder.Services.AddScoped<AufyFluentEmailFactory>();
        builder.Services.AddTransient<IAufyEmailConfirmationEmailSender<TUser>, EmailConfirmationEmailSender<TUser>>();
        builder.Services.AddTransient<IAufyPasswordResetEmailSender<TUser>, PasswordResetEmailSender<TUser>>();
        
        return builder;
    }
}