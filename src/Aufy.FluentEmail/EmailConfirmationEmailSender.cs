using System.Reflection;
using Aufy.Core;
using Aufy.Core.EmailSender;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aufy.FluentEmail;

public class EmailConfirmationEmailSender<TUser>(
    IOptions<FluentEmailOptions> fluentEmailOptions,
    AufyFluentEmailFactory emailFactory,
    ILogger<EmailConfirmationEmailSender<TUser>> logger) : IAufyEmailConfirmationEmailSender<TUser>
    where TUser : IdentityUser, IAufyUser
{
    protected virtual object PrepareEmailConfirmationModel(IdentityUser user, string confirmationLink)
    {
        return new
        {
            ConfirmationLink = confirmationLink,
            user.Email,
            user.UserName,
        };
    }

    public virtual async Task SendAsync(TUser user, string confirmationLink)
    {
        var email = emailFactory.CreateEmail();
        if (email is null)
        {
            return;
        }
        
        var emailSettings = fluentEmailOptions.Value.Emails.GetValueOrDefault(EmailType.Confirmation);

        email
            .SetFrom(fluentEmailOptions.Value.FromEmail, fluentEmailOptions.Value.FromName)
            .To(user.Email)
            .Subject(string.IsNullOrWhiteSpace(emailSettings?.Subject) ? "Confirm your email" : emailSettings.Subject);

        if (string.IsNullOrWhiteSpace(emailSettings?.TemplatePath))
        {
            email.UsingTemplateFromEmbedded(
                "Aufy.FluentEmail.Templates.EmailConfirmation.template.html",
                PrepareEmailConfirmationModel(user, confirmationLink),
                Assembly.GetAssembly(GetType()));
        }
        else
        {
            email.UsingTemplateFromFile(
                emailSettings.TemplatePath,
                PrepareEmailConfirmationModel(user, confirmationLink));
        }

        var res = await email.SendAsync();
        if (res.Successful)
        {
            logger.LogInformation("Email confirmation sent to {Email}", user.Email);
        }
        else
        {
            logger.LogError("Email confirmation failed to send to {Email}. Errors: {Errors}", user.Email,
                res.ErrorMessages);
        }
    }
}