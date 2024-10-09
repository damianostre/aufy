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
                    [FromServices] AufyUserManager<TUser> userManager,
                    [FromServices] AufySignInManager<TUser> signInManager,
                    [FromServices] ILogger<SignInExternalEndpoint<TUser>> logger,
                    HttpContext context,
                    IOptions<AufyOptions> options,
                    ClaimsPrincipal claimsPrincipal) =>
                {
                    await context.SignOutAsync(AufyAuthSchemeDefaults.SignInExternalScheme);
                    await context.SignOutAsync(AufyAuthSchemeDefaults.SignUpExternalScheme);
                    signInManager.UseCookie = useCookie ?? false;

                    var (checkLoginResult, checkError) = await userManager.CheckLogin(claimsPrincipal.Identity as ClaimsIdentity);
                    if (checkError is not null || checkLoginResult is null)
                    {
                        logger.LogError("Error checking login: {Error}", checkError);
                        return TypedResults.Problem("Error occurred");
                    }

                    var (providerKey, user) = checkLoginResult;
                    if (user is not null)
                    {
                        await signInManager.SignInAsync(user, new AuthenticationProperties(),
                            claimsPrincipal.Identity.AuthenticationType);

                        return TypedResults.Empty;
                    }

                    if (options.Value.EnableSignUp is false)
                    {
                        return TypedResults.Problem("Sign up is disabled");
                    }

                    if (options.Value.AutoAccountLinking)
                    {
                        if (claimsPrincipal.Identity is not ClaimsIdentity claimsIdentity)
                        {
                            return TypedResults.Problem("Claims identity is missing");
                        }

                        var (linkedUser, error) = await userManager.TryAutoLinkLoginAsync(claimsIdentity);
                        if (error is not null)
                        {
                            logger.LogError("Error linking login: {Error}", error);
                            return TypedResults.Problem("Error occurred");
                        }
                        // If the login was linked then Sign in
                        else if (linkedUser is not null)
                        {
                            await signInManager.SignInAsync(linkedUser, new AuthenticationProperties(),
                                claimsPrincipal.Identity.AuthenticationType);
                            return TypedResults.Empty;
                        }
                    }

                    if (AufyOptions.Internal.CustomExternalSignUpFlow)
                    {
                        logger.LogInformation("Cannot create user as custom external sign up flow is enabled");
                        return TypedResults.Problem("User not found");
                    }

                    var (newUser, problem) = await userManager.CreateUserWithLoginAsync(
                        providerKey, context, new DefaultSignupExternalRequest(), claimsPrincipal);
                    if (problem is not null)
                    {
                        return problem;
                    }

                    if (newUser is null)
                    {
                        logger.LogError("Failed to create user");
                        return TypedResults.Problem("Error occurred");
                    }

                    await signInManager.SignInAsync(newUser, new AuthenticationProperties(),
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

internal class DefaultSignupExternalRequest
{
}