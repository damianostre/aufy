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

public class SignInEndpoint<TUser> : IAuthEndpoint where TUser : IdentityUser, IAufyUser, new()
{
    public RouteHandlerBuilder Map(IEndpointRouteBuilder builder)
    {
        return builder.MapPost("/signin", async Task<Results<SignInHttpResult, ProblemHttpResult, EmptyHttpResult>>
            ([FromBody, Required] SignInRequest req,
                [FromServices] IOptions<AufyOptions> options,
                [FromServices] AufySignInManager<TUser> signInManager,
                [FromServices] ILogger<SignInEndpoint<TUser>> logger) =>
            {
                var result = await signInManager.SignInWithPasswordAsync(
                    req.Email,
                    req.Password,
                    AufyIdentityConstants.CookieScheme);
                
                if (!result.Succeeded)
                {
                    return TypedResults.Problem(result.ToValidationProblem());
                }

                return TypedResults.Empty;
            })
            .AddEndpointFilter<ValidationEndpointFilter<SignInRequest>>()
            .AllowAnonymous();
    }
}

public class SignInRequest
{
    [Required, EmailAddress] public required string Email { get; set; }
    [Required] public required string Password { get; set; }
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