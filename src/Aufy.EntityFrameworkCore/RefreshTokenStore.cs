using Aufy.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Aufy.EntityFrameworkCore;

public class RefreshTokenStore<TContext, TUser>(TContext context) : IRefreshTokenStore
    where TContext : DbContext, IAufyDbContext<TUser>
    where TUser : IdentityUser, IAufyUser
{
    public async Task<AufyRefreshToken?> FindByUserIdAsync(string userId, CancellationToken ct)
    {
        return await context.RefreshTokens.FindAsync(userId);
    }

    public async Task SaveAsync(AufyRefreshToken refreshToken, CancellationToken ct)
    {
        // We need to upsert the refresh token
        // Upsert operation is database specific, and it is not supported by EF Core
        // For now there is simple database agnostic solution
        var currentToken = await context.RefreshTokens.FindAsync(refreshToken.UserId, ct);
        if (currentToken is null)
        {
            await context.RefreshTokens.AddAsync(refreshToken, ct);
        }
        else
        {
            currentToken.RefreshToken = refreshToken.RefreshToken;
            currentToken.ExpiresAt = refreshToken.ExpiresAt;
        }

        await context.SaveChangesAsync(ct);
    }

    public Task DeleteByUserIdAsync(string userId)
    {
        return context.RefreshTokens.Where(t => t.UserId == userId).ExecuteDeleteAsync();
    }
}