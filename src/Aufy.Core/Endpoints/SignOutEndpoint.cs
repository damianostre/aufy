using System.Security.Claims;
using Aufy.Core.AuthSchemes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Aufy.Core.Endpoints;

public class SignOutEndpoint<TUser> : IAuthEndpoint where TUser : AufyUser
{
    public RouteHandlerBuilder Map(IEndpointRouteBuilder builder)
    {
        return builder.MapPost("/signout", async Task<Ok>
            (HttpContext context) =>
            {
                await context.SignOutAsync(AufyAuthSchemeDefaults.BearerSignInScheme);
                return TypedResults.Ok();
            })
            .RequireAuthorization();
    }
}