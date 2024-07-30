using System.Security.Claims;
using Aufy.Core.AuthSchemes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Aufy.Core.Endpoints;

public class TokenRefreshEndpoint<TUser> : IAuthEndpoint where TUser : IdentityUser, IAufyUser
{
    public RouteHandlerBuilder Map(IEndpointRouteBuilder builder)
    {
        return builder.MapPost(
                "/token/refresh",
                async Task<Results<SignInHttpResult, UnauthorizedHttpResult>> (
                    [FromServices] UserManager<TUser> manager,
                    [FromServices] IRefreshTokenManager refreshTokenManager,
                    HttpContext context) =>
                {
                    var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var token = context.User.FindFirstValue(AufyClaimTypes.RefreshToken);
                    
                    if (userId == null || token == null)
                    {
                        return TypedResults.Unauthorized();
                    }
                    
                    var user = await manager.FindByIdAsync(userId);
                    if (user == null)
                    {
                        return TypedResults.Unauthorized();
                    }

                    if (!await refreshTokenManager.ValidateAsync(user.Id, token))
                    {
                        return TypedResults.Unauthorized();
                    }

                    return TypedResults.SignIn(context.User,
                        authenticationScheme: AufyAuthSchemeDefaults.BearerTokenScheme);
                })
            .RequireAuthorization(b =>
            {
                b.RequireAuthenticatedUser();
                b.AddAuthenticationSchemes(AufyAuthSchemeDefaults.RefreshTokenScheme);
            });
    }
}