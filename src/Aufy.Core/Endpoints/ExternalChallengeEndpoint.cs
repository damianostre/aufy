using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Aufy.Core.Endpoints;

public class ExternalChallengeEndpoint<TUser> : IAuthEndpoint where TUser : AufyUser
{
    public RouteHandlerBuilder Map(IEndpointRouteBuilder builder)
    {
        return builder.MapGet("/external/challenge/{provider}",
            async Task<Results<ChallengeHttpResult, NotFound<string>>> (
                [FromRoute, Required] string provider,
                [FromQuery, Required] string callbackUrl,
                HttpContext ctx) =>
            {
                var scheme = (await ctx.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>()
                        .GetAllSchemesAsync())
                    .FirstOrDefault(s => s.Name.Equals(provider, StringComparison.OrdinalIgnoreCase));
                if (scheme == null)
                {
                    return TypedResults.NotFound("Provider not found");
                }

                return TypedResults.Challenge(new AuthenticationProperties
                {
                    RedirectUri = callbackUrl,
                }, new List<string> { scheme.Name });
            }).AllowAnonymous();
    }
}