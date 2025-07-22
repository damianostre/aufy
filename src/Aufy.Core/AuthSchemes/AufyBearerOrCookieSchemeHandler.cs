using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aufy.Core.AuthSchemes;

sealed class AufyBearerOrCookieSchemeHandler(
    IOptionsMonitor<PolicySchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : PolicySchemeHandler(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var bearerResult = await Context.AuthenticateAsync(AufyIdentityConstants.BearerScheme);

        // Only try to authenticate with the application cookie if there is no bearer token.
        if (!bearerResult.None)
        {
            return bearerResult;
        }

        // Cookie auth will return AuthenticateResult.NoResult() like bearer auth just did if there is no cookie.
        return await Context.AuthenticateAsync(AufyIdentityConstants.CookieScheme);
    }
}