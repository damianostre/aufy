using Aufy.Core;
using Aufy.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TestWebApi.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<TestUser>(options), IAufyDbContext<TestUser>
{
    public DbSet<AufyRefreshToken> RefreshTokens { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.ApplyAufyModel<TestUser>();
    }
}