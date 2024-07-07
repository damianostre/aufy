using System.Runtime.CompilerServices;
using Aufy.Core;
using Microsoft.EntityFrameworkCore;

namespace Aufy.EntityFrameworkCore;

public class RefreshTokenStore<TContext, TUser> : IRefreshTokenStore where TContext : AufyDbContext<TUser> where TUser : AufyUser, new()
{
    private readonly TContext _context;

    public RefreshTokenStore(TContext context)
    {
        _context = context;
    }

    public async Task<AufyRefreshToken?> FindByUserIdAsync(string userId, CancellationToken ct)
    {
        return await _context.RefreshTokens.FindAsync(userId);
    }

    public async Task SaveAsync(AufyRefreshToken refreshToken, CancellationToken ct)
    {
        // We need to upsert the refresh token
        // Upsert operation is database specific and it is not supported by EF Core
        // For now there is simple database agnostic solution
        var currentToken = await _context.RefreshTokens.FindAsync(refreshToken.UserId, ct);

        if (currentToken is null)
        {
            await _context.RefreshTokens.AddAsync(refreshToken, ct);
        }
        else
        {
            currentToken.RefreshToken = refreshToken.RefreshToken;
            currentToken.ExpiresAt = refreshToken.ExpiresAt;
        }

        await _context.SaveChangesAsync(ct);
    }

    public Task DeleteByUserIdAsync(string userId)
    {
        return _context.RefreshTokens.Where(t => t.UserId == userId).ExecuteDeleteAsync();
    }
}