using System.ComponentModel.DataAnnotations;
using Aufy.Core.AuthSchemes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Aufy.Core.Endpoints;

public class TokenEndpoint<TUser> : IAuthEndpoint where TUser : IdentityUser, IAufyUser, new()
{
    public RouteHandlerBuilder Map(IEndpointRouteBuilder builder)
    {
        return builder.MapPost("/token", async Task<Results<SignInHttpResult, ProblemHttpResult, EmptyHttpResult>>
            ([FromBody, Required] TokenRequest req,
                [FromServices] AufySignInManager<TUser> manager,
                [FromServices] ILogger<TokenEndpoint<TUser>> logger) =>
            {
                var authParams = AufySignInJwtBearerHandler.BuildParameters(req.SetTokenCookie, req.SetRefreshTokenCookie);
                var result = await manager.SignInWithPasswordAsync(
                    req.Email,
                    req.Password,
                    AufyIdentityConstants.BearerSignInScheme,
                    authenticationParameters: authParams);
                
                if (!result.Succeeded)
                {
                    return TypedResults.Problem(result.ToValidationProblem());
                }

                return TypedResults.Empty;
            })
            .AddEndpointFilter<ValidationEndpointFilter<TokenRequest>>()
            .AllowAnonymous();
    }
}

public class TokenRequest
{
    [Required, EmailAddress] public required string Email { get; set; }
    [Required] public required string Password { get; set; }
    public bool SetTokenCookie { get; set; } = true;
    public bool SetRefreshTokenCookie { get; set; } = true;
}