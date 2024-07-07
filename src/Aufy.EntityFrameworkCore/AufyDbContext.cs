using Aufy.Core;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Aufy.EntityFrameworkCore;

public class AufyDbContext<TUser> : IdentityDbContext<TUser> where TUser : AufyUser, new()
{
    public DbSet<AufyRefreshToken> RefreshTokens { get; set; }
    
    public AufyDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<AufyRefreshToken>(b =>
        {
            b.ToTable("AufyRefreshTokens");
            b.HasKey(x => x.UserId);
            b.Property(x => x.RefreshToken).IsRequired();
            b.Property(x => x.ExpiresAt).IsRequired();
        });
    }
}