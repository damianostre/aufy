using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aufy.Core.EmailSender;

public class AufyEmailSenderManager<TUser>(
    IServiceProvider serviceProvider,
    IOptions<IdentityOptions> identityOptions,
    ILogger<AufyEmailSenderManager<TUser>> logger)
    : IAufyEmailSenderManager<TUser>
    where TUser : AufyUser
{
    private readonly IdentityOptions _identityOptions = identityOptions.Value;
    
    private readonly IAufyEmailConfirmationEmailSender<TUser>? _confirmationSender = serviceProvider.GetService<IAufyEmailConfirmationEmailSender<TUser>>();
    private readonly IAufyPasswordResetEmailSender<TUser>? _passwordResetSender = serviceProvider.GetService<IAufyPasswordResetEmailSender<TUser>>();

    public Task SendEmailConfirmationAsync(TUser user, string confirmationLink)
    {
        if (!_identityOptions.SignIn.RequireConfirmedEmail)
        {
            logger.LogDebug("Email confirmation not required. Skipping email confirmation for {Email}", user.Email);
            return Task.CompletedTask;
        }
        
        if (user.EmailConfirmed)
        {
            logger.LogInformation("Email already confirmed for {Email}", user.Email);
            return Task.CompletedTask;
        }

        if (_confirmationSender is null)
        {
            logger.LogCritical("Email client is not configured. Cannot send email confirmation for {Email}",
                user.Email);
            return Task.CompletedTask;
        }

        return _confirmationSender.SendAsync(user, confirmationLink);
    }
    
    public Task SendPasswordResetAsync(TUser user, string resetLink)
    {
        if (_passwordResetSender is null)
        {
            logger.LogCritical("Email client is not configured. Cannot send password reset for {Email}",
                user.Email);
            return Task.CompletedTask;
        }

        return _passwordResetSender.SendAsync(user, resetLink);
    }
}

public interface IAufyEmailSenderManager<T> where T: AufyUser
{
    Task SendEmailConfirmationAsync(T user, string confirmationLink);
    Task SendPasswordResetAsync(T user, string resetLink);
}