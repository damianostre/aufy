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

public class SignInExternalEndpoint<TUser> : IAuthEndpoint where TUser : IdentityUser, IAufyUser, new()
{
    public RouteHandlerBuilder Map(IEndpointRouteBuilder builder)
    {
        return builder.MapPost("/signin/external",
                async Task<Results<EmptyHttpResult, UnauthorizedHttpResult, ProblemHttpResult>> (
                    [FromQuery] bool? useCookie,
                    [FromServices] AufySignInManager<TUser> signInManager,
                    [FromServices] ILogger<SignInExternalEndpoint<TUser>> logger,
                    [FromServices] UserManager<TUser> userManager,
                    HttpContext context,
                    ClaimsPrincipal claimsPrincipal,
                    IOptions<AufyOptions> options,
                    IServiceProvider serviceProvider) =>
                {
                    await context.SignOutAsync(AufyAuthSchemeDefaults.SignInExternalScheme);
                    await context.SignOutAsync(AufyAuthSchemeDefaults.SignUpExternalScheme);

                    var providerKey = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (claimsPrincipal.Identity?.AuthenticationType is null || providerKey is null)
                    {
                        logger.LogInformation("User not authenticated or missing name claim: {Claims}", context.User.Claims);
                        return TypedResults.Unauthorized();
                    }

                    var user = await signInManager.UserManager.FindByLoginAsync(claimsPrincipal.Identity.AuthenticationType, providerKey);
                    if (user == null)
                    {
                        var (newUser, problem) = await SignUpExternalEndpoint<TUser, SignUpExternalRequest>
                            .CreateUserAsync(
                                providerKey,
                                context,
                                new SignUpExternalRequest(),
                                claimsPrincipal,
                                serviceProvider,
                                userManager,
                                signInManager,
                                options,
                                logger);

                        if (problem is not null)
                        {
                            return problem;
                        }

                        if (newUser is null)
                        {
                            return TypedResults.Problem("There was an error creating user");
                        }

                        user = newUser;
                    }

                    signInManager.UseCookie = useCookie ?? false;
                    await signInManager.SignInAsync(user, new AuthenticationProperties(), claimsPrincipal.Identity.AuthenticationType);
                    
                    return TypedResults.Empty;
                })
            .RequireAuthorization(b =>
            {
                b.RequireAuthenticatedUser();
                b.AddAuthenticationSchemes(AufyAuthSchemeDefaults.SignInExternalScheme);
            });
    }
}