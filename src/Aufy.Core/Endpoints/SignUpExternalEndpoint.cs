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
        return builder.MapPost("/signup/external",
                async Task<Results<SignInHttpResult, BadRequest, ProblemHttpResult, UnauthorizedHttpResult,
                    EmptyHttpResult>>
                ([FromBody] TModel req,
                    [FromQuery] bool? useCookie,
                    [FromServices] AufyUserManager<TUser> userManager,
                    [FromServices] AufySignInManager<TUser> signInManager,
                    [FromServices] ILogger<SignUpExternalEndpoint<TUser, TModel>> logger,
                    ClaimsPrincipal claimsPrincipal,
                    IOptions<AufyOptions> options,
                    HttpContext context) =>
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
                        claimsPrincipal,
                        context,
                        signUpModel: req);

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
            .AddEndpointFilter<ValidationEndpointFilter<TModel>>()
            .RequireAuthorization(b =>
            {
                b.RequireAuthenticatedUser();
                b.AddAuthenticationSchemes(AufyAuthSchemeDefaults.SignUpExternalScheme);
            });
    }
}