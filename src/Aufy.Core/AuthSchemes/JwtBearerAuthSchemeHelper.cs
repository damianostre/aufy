using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Aufy.Core.AuthSchemes;

public static class JwtBearerAuthSchemeHelper
{
    public static void ConfigureBearerAuth(
        this JwtBearerOptions bearerOptions, AufyJwtBearerOptions aufyBearerOptions)
    {
        if (aufyBearerOptions.SigningKey is null)
        {
            throw new ArgumentNullException(nameof(aufyBearerOptions.SigningKey));
        }
        
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(aufyBearerOptions.SigningKey));
        bearerOptions.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = key,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidAudience = aufyBearerOptions.Audience,
            ValidIssuer = aufyBearerOptions.Issuer,
        };
        
        bearerOptions.Events ??= new JwtBearerEvents();
        bearerOptions.Events.OnAuthenticationFailed = context =>
        {
            if (context.Exception is SecurityTokenExpiredException)
            {
                context.Response.Headers["X-Token-Expired"] = "true";
            }

            return Task.CompletedTask;
        };

        bearerOptions.TokenValidationParameters.ValidateAudience =
            bearerOptions.TokenValidationParameters.ValidAudience is not null;
        bearerOptions.TokenValidationParameters.ValidateIssuer =
            bearerOptions.TokenValidationParameters.ValidIssuer is not null;
    }
}