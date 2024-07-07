namespace Aufy.Core;

/// <summary>
/// Represents a refresh token
/// </summary>
public class AufyRefreshToken
{
    public required string UserId { get; set; }
    public required string RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
}