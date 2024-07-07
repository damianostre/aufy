using Aufy.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Aufy.EntityFrameworkCore;

public static class AufyServiceBuilderExtensions
{
    public static AufyServiceBuilder<TUser> AddEntityFrameworkStore<TContext, TUser>(this AufyServiceBuilder<TUser> builder) where TUser : AufyUser, new()
        where TContext : AufyDbContext<TUser>
    {
        builder.Services.AddScoped<IRefreshTokenStore, RefreshTokenStore<TContext, TUser>>();
        builder.IdentityBuilder.AddEntityFrameworkStores<TContext>();

        return builder;
    }
}