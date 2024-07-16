using Aufy.Core;
using Microsoft.EntityFrameworkCore;

namespace Aufy.EntityFrameworkCore;

public static class ModelBuilderExtensions
{
    public static void ApplyAufyModel(this ModelBuilder builder)
    {
        builder.Entity<AufyRefreshToken>(b =>
        {
            b.ToTable("AufyRefreshTokens");
            b.HasKey(x => x.UserId);
            b.Property(x => x.RefreshToken).IsRequired();
            b.Property(x => x.ExpiresAt).IsRequired();
        });
    }
}