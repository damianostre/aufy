using System.Security.Claims;
using Aufy.Core.AuthSchemes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aufy.Core.Endpoints;

public class SignUpExternalEndpoint<TUser, TModel> : IAuthEndpoint
    where TModel : class
    where TUser : IdentityUser, IAufyUser, new()
{
    public RouteHandlerBuilder Map(IEndpointRouteBuilder builder)
    {
        return builder.MapPost("/signup/external/{authMode}",
                async Task<Results<SignInHttpResult, BadRequest, ProblemHttpResult, UnauthorizedHttpResult,
                    EmptyHttpResult>>
                ([FromBody] TModel req,
                    [FromBody] SignUpExternalTokenInfo tokenInfo,
                    [FromRoute] string authMode,
                    [FromServices] AufySignInManager<TUser> signInManager,
                    [FromServices] ILogger<SignUpExternalEndpoint<TUser, TModel>> logger,
                    [FromServices] ClaimsPrincipal claimsPrincipal,
                    [FromServices] IOptions<AufyOptions> options) =>
                {
                    if (options.Value.EnableSignUp is false)
                    {
                        logger.LogError("Sign up is disabled, endpoint should not be reachable");
                        return TypedResults.Problem("Error occurred");
                    }

                    if (AufyOptions.Internal.CustomExternalSignUpFlow is false)
                    {
                        logger.LogError("Custom external sign up flow is disabled, endpoint should not be reachable");
                        return TypedResults.Problem("Error occurred");
                    }
                    
                    var (user, problem) = await signInManager.HandleExternalAuthAsync(
                        claimsPrincipal, signUpModel: req);

                    if (problem is not null)
                    {
                        return problem;
                    }

                    if (user is null)
                    {
                        logger.LogError("User is null after handling external auth");
                        return TypedResults.Problem("Error occurred");
                    }

                    var isCookieAuth = IsCookieAuth(authMode);
                    var scheme = isCookieAuth ? AufyIdentityConstants.CookieScheme : AufyIdentityConstants.BearerSignInScheme;
                    var properties = isCookieAuth switch
                    {
                        true => new AuthenticationProperties
                        {
                            IsPersistent = true
                        },
                        false => new AuthenticationProperties(
                            null, 
                            AufySignInJwtBearerHandler.BuildParameters(tokenInfo.SetTokenCookie, tokenInfo.SetRefreshTokenCookie))
                    };

                    await signInManager.SignInWith(
                        scheme,
                        user, 
                        properties,
                        claimsPrincipal.Identity?.AuthenticationType ?? "External");

                    return TypedResults.Empty;
                })
            .RequireAuthorization(b =>
            {
                b.RequireAuthenticatedUser();
                b.AddAuthenticationSchemes(AufyIdentityConstants.ExternalSignUpScheme);
            });
    }
    
    private bool IsCookieAuth(string authMode)
    {
        var cookieAuth = authMode.Equals("cookie", StringComparison.InvariantCultureIgnoreCase);
        if (cookieAuth)
        {
            return true;
        }
        
        var bearerAuth = authMode.Equals("bearer", StringComparison.InvariantCultureIgnoreCase);
        if (bearerAuth)
        {
            return false;
        }
        
        throw new ArgumentException($"Invalid auth mode: {authMode}. Expected 'cookie' or 'bearer'.");
    }
}

public record SignUpExternalTokenInfo
{
    public bool SetTokenCookie { get; set; } = true;
    public bool SetRefreshTokenCookie { get; set; } = true;
}