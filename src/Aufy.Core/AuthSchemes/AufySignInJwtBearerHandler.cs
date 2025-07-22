using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
    private const string SetTokenCookieParamKey = "SetTokenCookie";
    private const string SetRefreshTokenCookieParamKey = "SetRefreshTokenCookie";

    protected override async Task HandleSignInAsync(ClaimsPrincipal user, AuthenticationProperties? properties)
    {
        var (token, expiresAt) = tokenService.CreateAccessToken(user);
        var refreshToken = await refreshTokenManager.CreateTokenAsync(user);
        var (refreshJwtToken, refreshExpiresAt) = tokenService.CreateBearerRefreshToken(refreshToken);
        var (setTokenCookie, setRefreshTokenCookie) = GetParameters(properties);

        if (setRefreshTokenCookie)
        {
            Context.Response.Cookies.Append(
                AufyIdentityConstants.RefreshTokenScheme,
                refreshJwtToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = refreshExpiresAt,
                });
        }

        if (setTokenCookie)
        {
            Context.Response.Cookies.Append(
                AufyIdentityConstants.BearerScheme,
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
            AccessToken = setTokenCookie ? null : token,
            ExpiresIn = (long) (expiresAt - DateTime.UtcNow).TotalSeconds,
            RefreshToken = refreshJwtToken
        };

        await Context.Response.WriteAsJsonAsync(
            accessTokenResponse, AccessTokenResponseJsonSerializerContext.Default.AccessTokenResponse);
    }

    protected override Task HandleSignOutAsync(AuthenticationProperties? properties)
    {
        refreshTokenManager.ClearTokenAsync(Context.User.FindFirstValue(ClaimTypes.NameIdentifier));
        Context.Response.Cookies.Delete(AufyIdentityConstants.RefreshTokenScheme);
        return Task.CompletedTask;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        throw new NotSupportedException();
    }

    public static Dictionary<string, object?> BuildParameters(bool setTokenCookie, bool setRefreshTokenCookie)
    {
        return new Dictionary<string, object?>
        {
            {SetTokenCookieParamKey, setTokenCookie},
            {SetRefreshTokenCookieParamKey, setRefreshTokenCookie},
        };
    }

    private (bool setTokenCookie, bool setRefreshTokenCookie) GetParameters(AuthenticationProperties? properties)
    {
        if (properties is null)
        {
            return (false, false);
        }

        var setTokenCookie = properties.GetParameter<bool?>(SetTokenCookieParamKey) ?? false;
        var setRefreshTokenCookie = properties.GetParameter<bool?>(SetRefreshTokenCookieParamKey) ?? false;
        return (setTokenCookie, setRefreshTokenCookie);
    }
}