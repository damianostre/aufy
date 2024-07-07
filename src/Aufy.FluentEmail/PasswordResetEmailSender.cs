using System.Reflection;
using Aufy.Core;
using Aufy.Core.EmailSender;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aufy.FluentEmail;

public class PasswordResetEmailSender<TUser>(
    IOptions<FluentEmailOptions> fluentEmailOptions,
    AufyFluentEmailFactory emailFactory,
    ILogger<EmailConfirmationEmailSender<TUser>> logger) : IAufyPasswordResetEmailSender<TUser>
    where TUser : AufyUser
{
    protected virtual object PrepareForgotPasswordModel(TUser user, string confirmationLink)
    {
        return new
        {
            ResetPasswordLink = confirmationLink,
            user.Email,
            user.UserName,
        };
    }
    
    public virtual async Task SendAsync(TUser user, string link)
    {
        var email = emailFactory.CreateEmail();
        if (email is null)
        {
            return;
        }
        
        var emailSettings = fluentEmailOptions.Value.Emails.GetValueOrDefault(EmailType.PasswordReset);

        email
            .SetFrom(fluentEmailOptions.Value.FromEmail, fluentEmailOptions.Value.FromName)
            .To(user.Email)
            .Subject(string.IsNullOrWhiteSpace(emailSettings?.Subject) ? "Password recovery" : emailSettings.Subject);

        if (string.IsNullOrWhiteSpace(emailSettings?.TemplatePath))
        {
            email.UsingTemplateFromEmbedded(
                "Aufy.FluentEmail.Templates.PasswordReset.template.html",
                PrepareForgotPasswordModel(user, link),
                Assembly.GetAssembly(GetType()));
        }
        else
        {
            email.UsingTemplateFromFile(
                emailSettings.TemplatePath,
                PrepareForgotPasswordModel(user, link));
        }

        var res = await email.SendAsync();
        if (res.Successful)
        {
            logger.LogInformation("Password recovery sent to {Email}", user.Email);
        }
        else
        {
            logger.LogError("Password recovery email failed to send to {Email}. Errors: {Errors}", user.Email,
                res.ErrorMessages);
        }
    }
}