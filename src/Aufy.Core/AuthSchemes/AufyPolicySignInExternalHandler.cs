using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aufy.Core.AuthSchemes;

public class AufyPolicySignInExternalHandler(
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
                AufyAuthSchemeDefaults.SignUpExternalScheme,
                user,
                properties);
        }

        return Context.SignInAsync(
            AufyAuthSchemeDefaults.SignInExternalScheme,
            user,
            properties);
    }
}