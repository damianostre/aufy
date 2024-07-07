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
    where TModel : SignUpExternalRequest where TUser : AufyUser, new()
{
    public RouteHandlerBuilder Map(IEndpointRouteBuilder builder)
    {
        return builder.MapPost("/signup/external",
                async Task<Results<SignInHttpResult, BadRequest, ProblemHttpResult, UnauthorizedHttpResult>>
                ([FromBody] TModel req,
                    [FromServices] SignInManager<TUser> signInManager,
                    [FromServices] UserManager<TUser> userManager,
                    [FromServices] ILogger<SignUpExternalEndpoint<TUser, TModel>> logger,
                    [FromServices] IOptions<AufyOptions> options,
                    [FromServices] IServiceProvider serviceProvider,
                    ClaimsPrincipal claimsPrincipal,
                    HttpContext context) =>
                {
                    await context.SignOutAsync(AufyAuthSchemeDefaults.SignUpExternalScheme);

                    if (claimsPrincipal.Identity?.IsAuthenticated != true)
                    {
                        logger.LogInformation("User {UserId} is not authenticated", claimsPrincipal.Identity?.Name);
                        return TypedResults.Unauthorized();
                    }

                    if (claimsPrincipal.Identity.AuthenticationType is null)
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

                    var login = await userManager.FindByLoginAsync(
                        claimsPrincipal.Identity.AuthenticationType,
                        providerKey);
                    if (login is not null)
                    {
                        logger.LogInformation(
                            "User {UserId} already has an account",
                            claimsPrincipal.Identity.Name);
                        return TypedResults.Problem("There was an error creating user");
                    }

                    var email = claimsPrincipal.FindFirst(ClaimTypes.Email);
                    var name = claimsPrincipal.FindFirst(ClaimTypes.Name);

                    var user = new TUser
                    {
                        UserName = name?.Value,
                        Email = email?.Value,
                        EmailConfirmed = true,
                    };

                    var events = serviceProvider.GetService<ISignUpExternalEndpointEvents<TUser, TModel>>();
                    if (events is not null)
                    {
                        var userCreatingRes = await events.UserCreatingAsync(req, context.Request, user);
                        if (userCreatingRes is not null)
                        {
                            return userCreatingRes;
                        }
                    }

                    var result = await userManager.CreateAsync(user);
                    if (!result.Succeeded)
                    {
                        logger.LogError("Error creating user: {Email}. Result: {Result}", user.Email, result);
                        return TypedResults.Problem(result.ToValidationProblem());
                    }

                    var schemes = await signInManager.GetExternalAuthenticationSchemesAsync();
                    var scheme = schemes.FirstOrDefault(x => x.Name == claimsPrincipal.Identity.AuthenticationType);
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

                        return TypedResults.Problem("There was an error creating user");
                    }

                    result = await userManager.AddToRolesAsync(user, options.Value.DefaultRoles);
                    if (!result.Succeeded)
                    {
                        logger.LogError("Error adding user to roles: {Result}", result);
                        return TypedResults.Problem("Error occured", statusCode: 500);
                    }

                    if (events is not null)
                    {
                        await events.UserCreatedAsync(req, context.Request, user);
                    }

                    var userPrincipal = await signInManager.CreateUserPrincipalAsync(user);
                    return TypedResults.SignIn(
                        userPrincipal,
                        authenticationScheme: AufyAuthSchemeDefaults.BearerSignInScheme);
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
    where TModel : SignUpExternalRequest where TUser : AufyUser, new()
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