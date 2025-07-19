using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aufy.Core.AuthSchemes;

public class AufyExternalCallbackPolicyHandler(
    IOptionsMonitor<PolicySchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : PolicySchemeHandler(options, logger, encoder)
{

    protected override Task HandleSignInAsync(ClaimsPrincipal user, AuthenticationProperties? properties)
    {
        properties ??= new AuthenticationProperties();
        
        var failed = properties.GetParameter<bool>("failed");
        if (failed)
        {
            Context.Response.Redirect(properties.RedirectUri);
            return Task.CompletedTask;
        }

        var signup = properties.GetParameter<bool>("signup");
        if (signup)
        {
            return Context.SignInAsync(
                AufyIdentityConstants.ExternalSignUpScheme,
                user,
                properties);
        }

        return Context.SignInAsync(
            AufyIdentityConstants.ExternalScheme,
            user,
            properties);
    }
}