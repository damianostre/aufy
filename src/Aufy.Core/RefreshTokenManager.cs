using System.Security.Claims;
using Microsoft.Extensions.Options;

namespace Aufy.Core;


/// <summary>
/// Default implementation of IRefreshTokenManager
/// </summary>
public class RefreshTokenManager : IRefreshTokenManager
{
    public IOptions<AufyOptions> Options { get; }
    private readonly IRefreshTokenStore _refreshTokenStore;

    public RefreshTokenManager(IRefreshTokenStore refreshTokenStore, IOptions<AufyOptions> options)
    {
        Options = options;
        _refreshTokenStore = refreshTokenStore;
    }

    public async Task<bool> ValidateAsync(string userId, string token, CancellationToken ct = default)
    {
        var refreshToken = await _refreshTokenStore.FindByUserIdAsync(userId, ct);

        //Validate the refresh token
        if (refreshToken is null
            || refreshToken.RefreshToken != token
            || refreshToken.ExpiresAt < DateTime.UtcNow)
        {
            return false;
        }

        return true;
    }

    public async Task<AufyRefreshToken> CreateTokenAsync(ClaimsPrincipal user, CancellationToken ct = default)
    {
        var id = user.FindFirst(ClaimTypes.NameIdentifier);
        if (id == null)
            throw new Exception("The user does not have a name identifier claim!");
        
        var refreshToken = new AufyRefreshToken
        {
            UserId = id.Value,
            RefreshToken = Guid.NewGuid().ToString(),
            ExpiresAt = DateTime.UtcNow.AddHours(Options.Value.JwtBearer.RefreshTokenExpireInHours)
        };

        await _refreshTokenStore.SaveAsync(refreshToken, ct);

        return refreshToken;
    }

    public Task ClearTokenAsync(string? userId)
    {
        if (userId is null) return Task.CompletedTask;

        return _refreshTokenStore.DeleteByUserIdAsync(userId);
    }
}

/// <summary>
/// Manager for refresh tokens
/// </summary>
public interface IRefreshTokenManager
{
    /// <summary>
    /// Validates a refresh token associated with a user
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="token"></param>
    /// <param name="ct"></param>
    /// <returns>True if the token is valid, false otherwise</returns>
    Task<bool> ValidateAsync(string userId, string token, CancellationToken ct = default);
    
    /// <summary>
    /// Creates a refresh token for a user
    /// </summary>
    /// <param name="user"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<AufyRefreshToken> CreateTokenAsync(ClaimsPrincipal user, CancellationToken ct = default);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task ClearTokenAsync(string? userId);
}
