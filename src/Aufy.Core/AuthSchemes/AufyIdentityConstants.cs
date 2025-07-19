using Microsoft.AspNetCore.Identity;

namespace Aufy.Core.AuthSchemes;

public static class AufyIdentityConstants
{
    public static readonly string CookieScheme = IdentityConstants.ApplicationScheme;
    public static readonly string BearerScheme = IdentityConstants.BearerScheme;
    public static readonly string CookieAndBearerScheme = "Aufy.CookieAndBearerScheme";
    public static readonly string BearerSignInScheme = "Aufy.BearerSignInScheme";
    public static readonly string RefreshTokenScheme = "Aufy.RefreshToken";
    public static readonly string ExternalCallbackPolicyScheme = "Aufy.ExternalCallbackScheme";
    public static readonly string ExternalScheme = IdentityConstants.ExternalScheme;
    public static readonly string ExternalSignUpScheme = "Aufy.ExternalSignUp";
}