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
using Microsoft.Extensions.Options;

namespace Aufy.Core.Endpoints;

public class LinkExternalLoginEndpoint<TUser> : IAuthEndpoint where TUser : IdentityUser, IAufyUser, new()
{
    public RouteHandlerBuilder Map(IEndpointRouteBuilder builder)
    {
        return builder.MapPost("/link/external",
                async Task<Results<EmptyHttpResult, UnauthorizedHttpResult, ProblemHttpResult>> (
                    [FromServices] AufyUserManager<TUser> userManager,
                    [FromServices] ILogger<LinkExternalLoginEndpoint<TUser>> logger,
                    HttpContext context) =>
                {
                    var singInExternalAuthenticateResult = await context.AuthenticateAsync(AufyAuthSchemeDefaults.SignInExternalScheme);
                    if (!singInExternalAuthenticateResult.Succeeded
                        || singInExternalAuthenticateResult.Failure is not null
                        || singInExternalAuthenticateResult.Principal is not { Identity: ClaimsIdentity identity })
                    {
                        logger.LogInformation(
                            singInExternalAuthenticateResult.Failure,
                            "Failed to authenticate with {Scheme}", AufyAuthSchemeDefaults.SignInExternalScheme);
                        return TypedResults.Problem("Error occurred");
                    }

                    await context.SignOutAsync(AufyAuthSchemeDefaults.SignInExternalScheme);
                    await context.SignOutAsync(AufyAuthSchemeDefaults.SignUpExternalScheme);

                    var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (userId is null)
                    {
                        logger.LogError("NameIdentifier claim is missing from user {Name}", context.User.Identity?.Name);
                        return TypedResults.Problem("Error occurred");
                    }

                    var (result, error) = await userManager.LinkLoginAsync(userId, identity);
                    if (error is not null || result is null)
                    {
                        logger.LogError("Error linking login: {Error}", error);
                        return TypedResults.Problem("Error occurred");
                    }

                    return TypedResults.Empty;
                })
            .RequireAuthorization();
    }
}