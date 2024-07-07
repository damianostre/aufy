namespace Aufy.Core.AuthSchemes;

public static class AufyAuthSchemeDefaults
{
    public const string BearerSignInScheme = "Aufy.BearerSignInCookieScheme";
    public const string BearerTokenScheme = "Aufy.BearerSignInTokenScheme";
    public const string RefreshTokenScheme = "Aufy.RefreshToken";
    public const string SignInExternalPolicyScheme = "Aufy.ExternalSignInDefaultScheme";
    public const string SignInExternalScheme = "Aufy.ExternalSignInScheme";
    public const string SignUpExternalScheme = "Aufy.ExternalSignUpScheme";
    
    public const string RefreshTokenCookieName = "Aufy.RefreshToken";
    public const string AccessTokenCookieName = "Aufy.AccessToken";
}