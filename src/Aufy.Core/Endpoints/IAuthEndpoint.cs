using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Aufy.Core.Endpoints;

public interface IAuthEndpoint : IEndpoint
{
}

public interface IAccountEndpoint : IEndpoint
{
}

public interface IEndpoint
{
    RouteHandlerBuilder Map(IEndpointRouteBuilder builder);
}