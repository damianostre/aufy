using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Aufy.Core.Endpoints;

public class ExternalProvidersEndpoint<TUser> : IAuthEndpoint where TUser : AufyUser
{
    public RouteHandlerBuilder Map(IEndpointRouteBuilder builder)
    {
        return builder.MapGet("/external/providers", async ([FromServices] SignInManager<TUser> signInManager) =>
        {
            var schemes = (await signInManager.GetExternalAuthenticationSchemesAsync())
                .Select(s => s.Name)
                .ToList();
            var res = new ExternalProvidersResponse
            {
                Providers = schemes
            };
        
            return TypedResults.Ok(res);
        }).AllowAnonymous();
    }
}

public class ExternalProvidersResponse
{
    public List<string> Providers { get; set; } = new();
}