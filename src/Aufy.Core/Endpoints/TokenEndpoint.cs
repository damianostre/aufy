using System.ComponentModel.DataAnnotations;
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

public class TokenEndpoint<TUser> : IAuthEndpoint where TUser : AufyUser
{
    public RouteHandlerBuilder Map(IEndpointRouteBuilder builder)
    {
        return builder.MapPost("/token", async Task<Results<SignInHttpResult, ProblemHttpResult, EmptyHttpResult>>
            ([FromBody, Required] TokenRequest req,
                [FromServices] SignInManager<TUser> manager,
                [FromServices] ILogger<TokenEndpoint<TUser>> logger) =>
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(req.Email);
                ArgumentException.ThrowIfNullOrWhiteSpace(req.Password);
                
                manager.AuthenticationScheme = AufyAuthSchemeDefaults.BearerTokenScheme;

                var result =
                    await manager.PasswordSignInAsync(req.Email, req.Password, isPersistent: false, lockoutOnFailure: true);
                if (!result.Succeeded)
                {
                    logger.LogInformation("User {Email} failed to sign in. Result: {Result}", req.Email, result);
                    return TypedResults.Problem(
                        "Invalid email or password", 
                        statusCode: StatusCodes.Status401Unauthorized);
                }

                return TypedResults.Empty;
            })
            .AddEndpointFilter<ValidationEndpointFilter<TokenRequest>>()
            .AllowAnonymous();
    }
}

public class TokenRequest
{
    [Required, EmailAddress] public string? Email { get; set; }
    [Required] public string? Password { get; set; }
}