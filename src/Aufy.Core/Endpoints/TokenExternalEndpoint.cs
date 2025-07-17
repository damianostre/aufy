using System.Security.Claims;
using Aufy.Core.AuthSchemes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Aufy.Core.Endpoints;

public class TokenExternalEndpoint<TUser> : IAuthEndpoint where TUser : IdentityUser, IAufyUser, new()
{
    public RouteHandlerBuilder Map(IEndpointRouteBuilder builder)
    {
        return builder.MapPost("/signin/external",
                async Task<Results<EmptyHttpResult, UnauthorizedHttpResult, ProblemHttpResult>> (
                    [FromQuery] bool? useCookie,
                    [FromServices] AufyUserManager<TUser> userManager,
                    [FromServices] AufySignInManager<TUser> signInManager,
                    [FromServices] ILogger<SignInExternalEndpoint<TUser>> logger,
                    HttpContext context,
                    ClaimsPrincipal claimsPrincipal) =>
                {                   
                    var (user, problem) = await signInManager.HandleExternalAuthAsync(
                        claimsPrincipal,
                        context,
                        signUpModel: new DefaultSignupExternalRequest());
                    
                    if (problem is not null)
                    {
                        return problem;
                    }

                    if (user is null)
                    {
                        logger.LogError("User is null after handling external auth");
                        return TypedResults.Problem("Error occurred");
                    }

                    await signInManager.SignInAsync(user, new AuthenticationProperties(),
                        claimsPrincipal.Identity.AuthenticationType);

                    return TypedResults.Empty;
                })
            .RequireAuthorization(b =>
            {
                b.RequireAuthenticatedUser();
                b.AddAuthenticationSchemes(AufyAuthSchemeDefaults.SignInExternalScheme);
            });
    }
}