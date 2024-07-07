using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Aufy.Core;

/// <summary>
/// Default implementation of <see cref="IJwtTokenService"/> 
/// </summary>
/// <param name="options"></param>
public class JwtTokenService(IOptions<AufyOptions> options) : IJwtTokenService
{
    public (string token, DateTime expiresAt) CreateAccessToken(ClaimsPrincipal user)
    {
        if (options.Value.JwtBearer.SigningKey is null)
        {
            throw new ArgumentNullException(nameof(options.Value.JwtBearer.SigningKey));
        }
        
        var expiresAt = DateTime.UtcNow.AddMinutes(options.Value.JwtBearer.AccessTokenExpiresInMinutes);
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = options.Value.JwtBearer.Issuer,
            Audience = options.Value.JwtBearer.Audience,
            IssuedAt = DateTime.UtcNow,
            Subject = new ClaimsIdentity(user.Claims),
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.ASCII.GetBytes(options.Value.JwtBearer.SigningKey)),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var handler = new JwtSecurityTokenHandler();
        return (handler.WriteToken(handler.CreateToken(descriptor)), expiresAt);
    }
    
    public (string token, DateTime expiresAt) CreateBearerRefreshToken(AufyRefreshToken token)
    {
        if (options.Value.JwtBearer.SigningKey is null)
        {
            throw new ArgumentNullException(nameof(options.Value.JwtBearer.SigningKey));
        }
        
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = options.Value.JwtBearer.Issuer,
            Audience = options.Value.JwtBearer.Audience,
            IssuedAt = DateTime.UtcNow,
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, token.UserId),
                new Claim(AufyClaimTypes.RefreshToken, token.RefreshToken)
            }),
            Expires = token.ExpiresAt,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.ASCII.GetBytes(options.Value.JwtBearer.SigningKey)),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var handler = new JwtSecurityTokenHandler();
        return (handler.WriteToken(handler.CreateToken(descriptor)), token.ExpiresAt);
    }
}


/// <summary>
/// Service for creating JWT tokens
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <returns>The JWT bearer token and the expiration date</returns>
    (string token, DateTime expiresAt) CreateAccessToken(ClaimsPrincipal user);
    
    /// <summary>
    /// Creates a bearer refresh token using the given refresh token created by the <see cref="IRefreshTokenManager"/>
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    (string token, DateTime expiresAt) CreateBearerRefreshToken(AufyRefreshToken token);
}