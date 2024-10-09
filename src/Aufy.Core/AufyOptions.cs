using Microsoft.AspNetCore.Authentication;

namespace Aufy.Core;

/// <summary>
/// Options for Aufy
/// </summary>
public class AufyOptions
{
    public const string SectionPath = "Aufy";
    
    public string AuthApiBasePath { get; set; } = "/api/auth";
    public string AccountApiBasePath { get; set; } = "/api/account";
    public string[] DefaultRoles { get; set; } = [];
    public bool AutoAccountLinking { get; set; } = true;

    public bool EnableEmailPasswordFlow { get; set; } = true;
    public bool EnableExternalProvidersFlow { get; set; } = true;
    public bool EnableSignUp { get; set; } = true;
    
    public ClientAppOptions ClientApp { get; set; } = new();
    public AufyJwtBearerOptions JwtBearer { get; set; } = new();
    public AufyProviders Providers { get; set; } = new();

    internal class Internal
    {
        public static bool CustomExternalSignUpFlow { get; set; }
    }
}

public class ClientAppOptions
{
    public string? BaseUrl { get; set; }
    public string EmailConfirmationPath { get; set; } = "/confirm-email";
    public string PasswordResetPath { get; set; } = "/reset-password";
}

public class AufyJwtBearerOptions : AuthenticationSchemeOptions
{
    public const string SectionPath = "Aufy:JwtBearer";
    
    public string? SigningKey { get; set; }
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
    public int AccessTokenExpiresInMinutes { get; set; } = 30;
    public int RefreshTokenExpireInHours { get; set; } = 72;
}

public class AufyProviders : Dictionary<string, AufyProviderInfo>
{
}

public class AufyProviderInfo
{
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string[]? Scopes { get; set; }
}