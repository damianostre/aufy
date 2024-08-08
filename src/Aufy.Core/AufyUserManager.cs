using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aufy.Core;

public class AufyUserManager<TUser>(
    UserManager<TUser> userManager,
    SignInManager<TUser> signInManager,
    IOptions<AufyOptions> options,
    ILogger<AufyUserManager<TUser>> logger
    ) : IAufyUserManager
    where TUser : IdentityUser, IAufyUser
{
    public async Task<bool> UserWithLoginExistsAsync(string provider, string providerKey)
    {
        var user = await userManager.FindByLoginAsync(provider, providerKey);
        return user != null;
    }

    public async Task<bool> TryLinkLoginAsync(ClaimsIdentity claims, AuthenticationScheme scheme)
    {
        if (options.Value.AutoAccountLinking is false)
        {
            logger.LogDebug("Account linking is disabled");
            return false;
        }

        var email = claims.FindFirst(ClaimTypes.Email);
        var user = email?.Value is not null ? await userManager.FindByEmailAsync(email.Value) : null;
        if (user is null)
        {
            logger.LogDebug("Login cannot be linked as no user with email {Email} exists", email?.Value);
            return false;
        }

        var providerKeyClaim = claims.FindFirst(ClaimTypes.NameIdentifier);
        if (providerKeyClaim is null)
        {
            throw new("NameIdentifier claim is missing");
        }

        var result = await userManager.AddLoginAsync(
            user, new UserLoginInfo(scheme.Name, providerKeyClaim.Value, scheme.DisplayName));
        if (!result.Succeeded)
        {
            logger.LogError(
                "Failed to add login info to user {UserId}: {Errors}",
                user.Id,
                string.Join(", ", result.Errors.Select(e => e.Description)));
            throw new("Failed to add login info to user");
        }

        return true;
    }
}

public interface IAufyUserManager
{
    Task<bool> UserWithLoginExistsAsync(string provider, string providerKey);
    Task<bool> TryLinkLoginAsync(ClaimsIdentity claims, AuthenticationScheme scheme);
}