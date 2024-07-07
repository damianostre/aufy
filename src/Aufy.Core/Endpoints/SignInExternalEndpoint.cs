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

public class SignInExternalEndpoint<TUser> : IAuthEndpoint where TUser : AufyUser
{
    public RouteHandlerBuilder Map(IEndpointRouteBuilder builder)
    {
        return builder.MapPost("/signin/external",
                async Task<Results<SignInHttpResult, UnauthorizedHttpResult>> (
                    [FromServices] SignInManager<TUser> manager,
                    [FromServices] ILogger<SignInExternalEndpoint<TUser>> logger,
                    HttpContext context) =>
                {
                    await context.SignOutAsync(AufyAuthSchemeDefaults.SignInExternalScheme);
                    await context.SignOutAsync(AufyAuthSchemeDefaults.SignUpExternalScheme);

                    var providerKey = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (context.User.Identity?.AuthenticationType is null || providerKey is null)
                    {
                        logger.LogInformation("User not authenticated or missing name claim: {Claims}", context.User.Claims);
                        return TypedResults.Unauthorized();
                    }
                    
                    var user = await manager.UserManager.FindByLoginAsync(context.User.Identity.AuthenticationType, providerKey);

                    if (user == null)
                    {
                        return TypedResults.Unauthorized();
                    }

                    var userPrincipal = await manager.CreateUserPrincipalAsync(user);
                    return TypedResults.SignIn(
                            userPrincipal,
                            authenticationScheme: AufyAuthSchemeDefaults.BearerSignInScheme);
                })
            .RequireAuthorization(b =>
            {
                b.RequireAuthenticatedUser();
                b.AddAuthenticationSchemes(AufyAuthSchemeDefaults.SignInExternalScheme);
            });
    }
}