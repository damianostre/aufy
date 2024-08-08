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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aufy.Core.Endpoints;

public class SignUpExternalEndpoint<TUser, TModel> : IAuthEndpoint
    where TModel : SignUpExternalRequest where TUser : IdentityUser, IAufyUser, new()
{
    public RouteHandlerBuilder Map(IEndpointRouteBuilder builder)
    {
        return builder.MapPost("/signup/external",
                async Task<Results<SignInHttpResult, BadRequest, ProblemHttpResult, UnauthorizedHttpResult,
                    EmptyHttpResult>>
                ([FromBody] TModel req,
                    [FromQuery] bool? useCookie,
                    [FromServices] AufySignInManager<TUser> signInManager,
                    [FromServices] UserManager<TUser> userManager,
                    [FromServices] ILogger<SignUpExternalEndpoint<TUser, TModel>> logger,
                    [FromServices] IOptions<AufyOptions> options,
                    [FromServices] IServiceProvider serviceProvider,
                    ClaimsPrincipal claimsPrincipal,
                    HttpContext context) =>
                {
                    await context.SignOutAsync(AufyAuthSchemeDefaults.SignInExternalScheme);
                    await context.SignOutAsync(AufyAuthSchemeDefaults.SignUpExternalScheme);

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

                    var (user, problem) = await CreateUserAsync(
                        providerKey,
                        context,
                        req,
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

    public static async Task<(TUser? user, ProblemHttpResult? problem)> CreateUserAsync(
        string providerKey,
        HttpContext context,
        TModel req,
        ClaimsPrincipal claimsPrincipal,
        IServiceProvider serviceProvider,
        UserManager<TUser> userManager,
        AufySignInManager<TUser> signInManager,
        IOptions<AufyOptions> options,
        ILogger logger)
    {
        var email = claimsPrincipal.FindFirst(ClaimTypes.Email);
        var name = claimsPrincipal.FindFirst(ClaimTypes.Name);
        var user = new TUser
        {
            UserName = name?.Value,
            Email = email?.Value,
            EmailConfirmed = false,
        };

        var events = serviceProvider.GetService<ISignUpExternalEndpointEvents<TUser, TModel>>();
        if (events is not null)
        {
            var userCreatingProblem = await events.UserCreatingAsync(req, context.Request, user);
            if (userCreatingProblem is not null)
            {
                return (null, userCreatingProblem);
            }
        }

        var result = await userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            logger.LogError("Error creating user: {Email}. Result: {Result}", user.Email, result);
            return (null, TypedResults.Problem(result.ToValidationProblem()));
        }

        var schemes = await signInManager.GetExternalAuthenticationSchemesAsync();
        var scheme = schemes.FirstOrDefault(x => x.Name == claimsPrincipal.Identity?.AuthenticationType);
        result = await userManager.AddLoginAsync(
            user,
            new UserLoginInfo(
                claimsPrincipal.Identity.AuthenticationType,
                providerKey,
                scheme?.DisplayName));

        if (!result.Succeeded)
        {
            logger.LogError(
                "Failed to add login info to user {UserId}: {Errors}",
                user.Id,
                string.Join(", ", result.Errors.Select(e => e.Description)));

            return (null, TypedResults.Problem("There was an error creating user"));
        }

        result = await userManager.AddToRolesAsync(user, options.Value.DefaultRoles);
        if (!result.Succeeded)
        {
            logger.LogError("Error adding user to roles: {Result}", result);
            return (null, TypedResults.Problem("Error occured", statusCode: 500));
        }

        if (events is not null)
        {
            await events.UserCreatedAsync(req, context.Request, user);
        }

        return (user, null);
    }
}

/// <summary>
/// Extension point for the SignUpExternalEndpoint.
/// </summary>
/// <typeparam name="TUser"></typeparam>
/// <typeparam name="TModel"></typeparam>
public interface ISignUpExternalEndpointEvents<TUser, TModel>
    where TModel : SignUpExternalRequest where TUser : IAufyUser, new()
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