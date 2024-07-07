using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Aufy.Core.AuthSchemes;

public class AufySignInJwtBearerHandler(
    IOptionsMonitor<AufyJwtBearerOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IOptions<AufyOptions> aufyOptions,
    IJwtTokenService tokenService,
    IRefreshTokenManager refreshTokenManager)
    : SignInAuthenticationHandler<AufyJwtBearerOptions>(options, logger, encoder)
{
    protected override async Task HandleSignInAsync(ClaimsPrincipal user, AuthenticationProperties? properties)
    {
        var (token, expiresAt) = tokenService.CreateAccessToken(user);
        var refreshToken = await refreshTokenManager.CreateTokenAsync(user);
        var (refreshJwtToken, refreshExpiresAt) = tokenService.CreateBearerRefreshToken(refreshToken);

        Context.Response.Cookies.Append(
            AufyAuthSchemeDefaults.RefreshTokenCookieName,
            refreshJwtToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = refreshExpiresAt,
            });
        
        if (aufyOptions.Value.SaveAccessTokenInCookie)
        {
            Context.Response.Cookies.Append(
                AufyAuthSchemeDefaults.AccessTokenCookieName,
                token,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = expiresAt,
                });
        }
        
        var accessTokenResponse = new AccessTokenResponse
        {
            AccessToken = token,
            ExpiresIn = (long)(expiresAt - DateTime.UtcNow).TotalSeconds,
        };

        await Context.Response.WriteAsJsonAsync(
            accessTokenResponse, AccessTokenResponseJsonSerializerContext.Default.AccessTokenResponse);
    }

    protected override Task HandleSignOutAsync(AuthenticationProperties? properties)
    {
        refreshTokenManager.ClearTokenAsync(Context.User.FindFirstValue(ClaimTypes.NameIdentifier));
        Context.Response.Cookies.Delete(AufyAuthSchemeDefaults.RefreshTokenCookieName);
        return Task.CompletedTask;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        throw new NotSupportedException();
    }
}