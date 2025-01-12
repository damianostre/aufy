using Aufy.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Aufy.EntityFrameworkCore;

public static class ModelBuilderExtensions
{
    public static void ApplyAufyModel<TUser>(this ModelBuilder builder) where TUser : IdentityUser, IAufyUser
    {
        builder.Entity<TUser>(b =>
        {
            b.HasMany(x => x.Roles).WithMany().UsingEntity<IdentityUserRole<string>>(
                x => x.HasOne<IdentityRole>().WithMany().HasForeignKey(m => m.RoleId),
                x => x.HasOne<TUser>().WithMany().HasForeignKey(m => m.UserId)
            );
        });

        builder.Entity<AufyRefreshToken>(b =>
        {
            b.ToTable("AufyRefreshTokens");
            b.HasKey(x => x.UserId);
            b.Property(x => x.RefreshToken).IsRequired();
            b.Property(x => x.ExpiresAt).IsRequired();
        });
    }
}