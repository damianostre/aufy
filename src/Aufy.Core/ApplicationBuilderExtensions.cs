using Aufy.Core.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aufy.Core;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Registers all Aufy endpoints
    /// </summary>
    /// <param name="app"></param>
    /// <param name="configure"></param>
    public static void MapAufyEndpoints(this WebApplication app, Action<EndpointsConfiguration>? configure = null)
    {
        var opts = app.Services.GetRequiredService<IOptions<AufyOptions>>().Value;
        var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
        var authGroup = app.MapGroup(opts.AuthApiBasePath).WithTags("Auth");
        var accountGroup = app.MapGroup(opts.AccountApiBasePath).WithTags("Account").RequireAuthorization();

        var routes = new Dictionary<IEndpoint, RouteHandlerBuilder>();
        foreach (var endpoint in app.Services.GetRequiredService<IEnumerable<IAuthEndpoint>>())
        {
            var route = endpoint.Map(authGroup).WithOpenApi();
            routes.Add(endpoint, route);
        }
        
        foreach (var endpoint in app.Services.GetRequiredService<IEnumerable<IAccountEndpoint>>())
        {
            var route = endpoint.Map(accountGroup).WithOpenApi();
            routes.Add(endpoint, route);
        }
        
        var c = new EndpointsConfiguration(authGroup, accountGroup, routes, loggerFactory);
        configure?.Invoke(c);
    }
}

public class EndpointsConfiguration(
    RouteGroupBuilder authGroup,
    RouteGroupBuilder accountGroup,
    Dictionary<IEndpoint, RouteHandlerBuilder> endpoints,
    ILoggerFactory loggerFactory)
{
    private readonly ILogger<EndpointsConfiguration> _logger = loggerFactory.CreateLogger<EndpointsConfiguration>();

    public void ConfigureAuthRouteGroup(Action<RouteGroupBuilder> action)
    {
        action(authGroup);
    }
    
    public void ConfigureAccountRouteGroup(Action<RouteGroupBuilder> action)
    {
        action(accountGroup);
    }
    
    public void ConfigureRoute<T>(Action<RouteHandlerBuilder> action) where T: IAuthEndpoint
    {
        var endpoint = endpoints.Keys.OfType<T>().FirstOrDefault();
        if (endpoint is null)
        {
            _logger.LogWarning("Endpoint {Endpoint} is not registered. Configuration will be skipped", typeof(T).Name);
            return;
        }

        action(endpoints[endpoint]);
    }
}