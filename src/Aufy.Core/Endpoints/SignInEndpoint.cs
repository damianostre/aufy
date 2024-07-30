using System.ComponentModel.DataAnnotations;
using Aufy.Core.AuthSchemes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Aufy.Core.Endpoints;

public class SignInEndpoint<TUser> : IAuthEndpoint where TUser : IdentityUser, IAufyUser
{
    public RouteHandlerBuilder Map(IEndpointRouteBuilder builder)
    {
        return builder.MapPost("/signin", async Task<Results<SignInHttpResult, ProblemHttpResult, EmptyHttpResult>>
            ([FromBody, Required] SignInRequest req,
                [FromQuery] bool? useCookie,
                HttpContext context,
                [FromServices] IOptions<AufyOptions> options,
                [FromServices] AufySignInManager<TUser> signInManager,
                [FromServices] UserManager<TUser> userManager,
                [FromServices] IServiceProvider serviceProvider,
                [FromServices] ILogger<SignInEndpoint<TUser>> logger) =>
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(req.Email);
                ArgumentException.ThrowIfNullOrWhiteSpace(req.Password);
                
                var events = serviceProvider.GetService<ISignInEndpointEvents<TUser>>();
                
                var user = await userManager.FindByEmailAsync(req.Email);
                if (user == null)
                {
                    if (events is not null)
                    {
                        await events.UserNotFound(req, context);
                    }
                    
                    logger.LogInformation("User {Email} failed to sign in. Reason: User not found", req.Email);
                    return TypedResults.Problem(SignInResult.Failed.ToValidationProblem());
                }
                
                signInManager.UseCookie = useCookie ?? false;
                var result = await signInManager.PasswordSignInAsync(
                    user, req.Password, isPersistent: false, lockoutOnFailure: true);
                
                if (!result.Succeeded)
                {
                    if (events is not null)
                    {
                        await events.SignInFailedAsync(req, context, result);
                    }
                    
                    logger.LogInformation("User {Email} failed to sign in. Result: {Result}", req.Email, result);
                    return TypedResults.Problem(result.ToValidationProblem());
                }
                
                if (events is not null)
                {
                    await events.SignInSucceededAsync(user, context);
                }
                
                logger.LogInformation("User {Email} signed in", req.Email);

                return TypedResults.Empty;
            })
            .AddEndpointFilter<ValidationEndpointFilter<SignInRequest>>()
            .AllowAnonymous();
    }
}

public class SignInRequest
{
    [Required, EmailAddress] public string? Email { get; set; }
    [Required] public string? Password { get; set; }
}

/// <summary>
/// Extension point for the SignInEndpoint.
/// </summary>
/// <typeparam name="TUser"></typeparam>
public interface ISignInEndpointEvents<in TUser> where TUser : IAufyUser
{
    Task SignInSucceededAsync(TUser user, HttpContext context);
    
    /// <summary>
    /// Called when a user fails to sign in.
    /// </summary>
    Task SignInFailedAsync(SignInRequest request, HttpContext context, SignInResult result);
    
    /// <summary>
    /// User not found.
    /// </summary>
    Task UserNotFound(SignInRequest request, HttpContext context);
}