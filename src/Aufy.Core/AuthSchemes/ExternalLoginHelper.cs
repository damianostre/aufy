using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Aufy.Core.AuthSchemes;

public static class ExternalLoginHelper
{
    public static AuthenticationBuilder AddProviderIfConfigured(this AuthenticationBuilder builder, string providerName,
        AufyOptions opts, Action<AuthenticationBuilder> builderAction)
    {
        if (!opts.Providers.TryGetValue(providerName, out var provider))
        {
            return builder;
        }

        if (string.IsNullOrWhiteSpace(provider.ClientId) || string.IsNullOrWhiteSpace(provider.ClientSecret))
        {
            return builder;
        }

        builderAction(builder);

        return builder;
    }

    public static void Configure(this OAuthOptions oauth, string scheme, AufyOptions aufyOptions)
    {
        if (!aufyOptions.Providers.TryGetValue(scheme, out var provider))
        {
            throw new($"Provider {scheme} is not configured");
        }
        
        oauth.ClientId = provider.ClientId ?? throw new($"ClientId for provider {scheme} is not configured");
        oauth.ClientSecret = provider.ClientSecret ?? throw new($"ClientSecret for provider {scheme} is not configured");
        oauth.CallbackPath = aufyOptions.AuthApiBasePath + "/external/callback/" + scheme.ToLower();
        oauth.SignInScheme = AufyAuthSchemeDefaults.SignInExternalPolicyScheme;
        
        foreach (var scope in provider.Scopes ?? [])
        {
            oauth.Scope.Add(scope);
        }

        oauth.Events.OnCreatingTicket = context => context.OnCreatingTicket();
        oauth.Events.OnRemoteFailure = context =>
        {
            context.HandleResponse();
            
            context.Properties ??= new();
            
            var redirectUri = context.Properties.RedirectUri + "?failed=true";
            context.Response.Redirect(redirectUri);
            
            return Task.CompletedTask;
        };
    }

    private static async Task OnCreatingTicket(this OAuthCreatingTicketContext context)
    {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<OAuthCreatingTicketContext>>();
        try
        {
            var userManager = context.HttpContext.RequestServices.GetRequiredService<IAufyUserManager>();
            var providerKey = context.Identity?.FindFirst(ClaimTypes.NameIdentifier);
            if (providerKey is null)
            {
                throw new("NameIdentifier claim is missing");
            }
        
            var exists = await userManager.UserWithLoginExistsAsync(context.Scheme.Name, providerKey.Value);
            if (exists is false)
            {
                context.Properties.SetParameter("signup", true);
                context.Properties.RedirectUri += "?signup=true";
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while creating ticket");
            
            // Context is lost if there is an exception, so we need to handle it here
            context.Properties.RedirectUri += "?failed=true";
            context.Properties.SetParameter("failed", true);
        }
    }
}