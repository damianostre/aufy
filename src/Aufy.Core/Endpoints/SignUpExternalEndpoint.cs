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
                    await context.SignOutAsync(AufyAuthSchemeDefaults.SignInExternalScheme);
                    await context.SignOutAsync(AufyAuthSchemeDefaults.SignUpExternalScheme);

                    if (options.Value.EnableSignUp is false)
                    {
                        throw new("Sign up is disabled. Endpoint should not be reachable");
                    }

                    if (claimsPrincipal.Identity?.AuthenticationType is null)
                    {
                        logger.LogInformation("User {UserId} has no authentication type",
                            claimsPrincipal.Identity?.Name);
                        return TypedResults.Unauthorized();
                    }

                    var providerKey = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (providerKey is null)
                    {
                        throw new("NameIdentifier claim is missing");
                    }

                    var loginUser = await userManager.FindByLoginAsync(
                        claimsPrincipal.Identity.AuthenticationType,
                        providerKey);
                    if (loginUser is not null)
                    {
                        logger.LogInformation(
                            "User {UserId} already has an account",
                            claimsPrincipal.Identity.Name);
                        return TypedResults.Problem("There was an error creating user");
                    }

                    var (user, problem) = await userManager.CreateUserWithLoginAsync(
                        providerKey, context, req, claimsPrincipal);

                    if (problem is not null)
                    {
                        return problem;
                    }

                    if (user is null)
                    {
                        return TypedResults.Problem("There was an error creating user");
                    }

                    signInManager.UseCookie = useCookie ?? false;
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

/// <summary>
/// Extension point for the SignUpExternalEndpoint.
/// </summary>
/// <typeparam name="TUser"></typeparam>
/// <typeparam name="TModel"></typeparam>
public interface ISignUpExternalEndpointEvents<TUser, TModel>
    where TModel : class
    where TUser : IAufyUser, new()
{
    /// <summary>
    /// Called when a user is being created. <br/>
    /// Return a ProblemHttpResult if the user can't be created.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="httpRequest"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    Task<ProblemHttpResult?> UserCreatingAsync(TModel model, HttpRequest httpRequest, TUser user)
    {
        return Task.FromResult<ProblemHttpResult?>(null);
    }

    /// <summary>
    /// Called when a user is created and saved to the database.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="httpRequest"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    Task UserCreatedAsync(TModel model, HttpRequest httpRequest, TUser user)
    {
        return Task.CompletedTask;
    }
}

public class SignUpExternalRequest
{
}