using Aufy.Core;
using Aufy.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<MyUser>(options), IAufyDbContext<MyUser>
{
    public DbSet<AufyRefreshToken> RefreshTokens { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.ApplyAufyModel<MyUser>();
    }
}