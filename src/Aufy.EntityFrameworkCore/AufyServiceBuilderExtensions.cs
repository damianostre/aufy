using Aufy.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Aufy.EntityFrameworkCore;

public static class AufyServiceBuilderExtensions
{
    public static AufyServiceBuilder<TUser> AddEntityFrameworkStore<TContext, TUser>(this AufyServiceBuilder<TUser> builder) 
        where TUser : IdentityUser, IAufyUser, new()
        where TContext : DbContext, IAufyDbContext<TUser>
    {
        builder.IdentityBuilder.AddEntityFrameworkStores<TContext>();
        builder.Services.AddScoped<IRefreshTokenStore, RefreshTokenStore<TContext, TUser>>();

        return builder;
    }
}