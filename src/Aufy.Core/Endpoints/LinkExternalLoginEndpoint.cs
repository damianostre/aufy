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

public class LinkExternalLoginEndpoint<TUser> : IAccountEndpoint where TUser : IdentityUser, IAufyUser, new()
{
    public RouteHandlerBuilder Map(IEndpointRouteBuilder builder)
    {
        return builder.MapPost("/link/external",
                async Task<Results<Ok<AccountInfoResponse>, UnauthorizedHttpResult, ProblemHttpResult>> (
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

                    var (user, error) = await userManager.LinkLoginAsync(userId, identity);
                    if (error is not null || user is null)
                    {
                        logger.LogError("Error linking login: {Error}", error);
                        return TypedResults.Problem("Error occurred");
                    }

                    var logins = await userManager.GetLoginsAsync(user);
                    var roles = await userManager.GetRolesAsync(user);

                    var res = new AccountInfoResponse
                    {
                        Email = user.Email,
                        Username = user.UserName,
                        HasPassword = await userManager.HasPasswordAsync(user),
                        Roles = roles.ToList(),
                        Logins = logins.Select(l => l.LoginProvider).ToList()
                    };

                    return TypedResults.Ok(res);
                })
            .RequireAuthorization();
    }
}