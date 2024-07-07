using System.Text.Json.Serialization;

namespace Aufy.Core.AuthSchemes;

public class AccessTokenResponse
{
    [JsonPropertyName("token_type")]
    public string TokenType { get; } = "Bearer";
    
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; set; }
    
    [JsonPropertyName("expires_in")]
    public long ExpiresIn { get; set; }
    
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }
}

[JsonSerializable(typeof(AccessTokenResponse))]
sealed partial class AccessTokenResponseJsonSerializerContext : JsonSerializerContext
{
}