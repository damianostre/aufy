using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Aufy.Core.AuthSchemes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SignInResult = Microsoft.AspNetCore.Mvc.SignInResult;

namespace Aufy.Core.Endpoints;

public class SignInRefreshEndpoint<TUser> : IAuthEndpoint where TUser : AufyUser
{
    public RouteHandlerBuilder Map(IEndpointRouteBuilder builder)
    {
        return builder.MapPost(
                "/signin/refresh",
                async Task<Results<SignInHttpResult, UnauthorizedHttpResult, EmptyHttpResult>> ( 
                    [FromQuery] bool? useCookie,
                    [FromServices] UserManager<TUser> manager,
                    [FromServices] IRefreshTokenManager refreshTokenManager,
                    [FromServices] AufySignInManager<TUser> signInManager,
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

                    signInManager.UseCookie = useCookie ?? false;
                    await signInManager.SignInAsync(user, new AuthenticationProperties(), context.User.Identity.AuthenticationType);
                    
                    return TypedResults.Empty;
                })
            .RequireAuthorization(b =>
            {
                b.RequireAuthenticatedUser();
                b.AddAuthenticationSchemes(AufyAuthSchemeDefaults.RefreshTokenScheme);
            });
    }
}