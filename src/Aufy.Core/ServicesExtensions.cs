using Aufy.Core.AuthSchemes;
using Aufy.Core.EmailSender;
using Aufy.Core.Endpoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aufy.Core;

/// <summary>
///
/// </summary>
public static class ServicesExtensions
{
    /// <summary>
    /// Registers Aufy services and endpoints
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="configureOptions"></param>
    /// <typeparam name="TUser"></typeparam>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static AufyServiceBuilder<TUser> AddAufy<TUser>(
        this IServiceCollection services, IConfiguration configuration, Action<AufyOptions>? configureOptions = null)
        where TUser : AufyUser, new()
    {
        var opts = configuration.GetSection(AufyOptions.SectionPath).Get<AufyOptions>();
        if (opts is null)
        {
            throw new("AufyOptions is not configured in appsettings.json");
        }
        
        configureOptions?.Invoke(opts);

        services.AddOptions<AufyOptions>().BindConfiguration(AufyOptions.SectionPath).Configure(o => configureOptions?.Invoke(o));
        services.AddOptions<AufyJwtBearerOptions>().BindConfiguration(AufyJwtBearerOptions.SectionPath).Configure(o =>
        {
            o.Issuer = opts.JwtBearer.Issuer;
            o.Audience = opts.JwtBearer.Audience;
            o.SigningKey = opts.JwtBearer.SigningKey;
        });

        services.AddScoped<IRefreshTokenManager, RefreshTokenManager>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAufyEmailSenderManager<TUser>, AufyEmailSenderManager<TUser>>();
        services.AddScoped<IAufyUserManager, AufyUserManager<TUser>>();

        if (opts.EnableEmailPasswordFlow)
        {
            services.AddSingleton<IAuthEndpoint, SignInEndpoint<TUser>>();
            services.AddSingleton<IAuthEndpoint, TokenRefreshEndpoint<TUser>>();
            services.AddSingleton<IAuthEndpoint, TokenEndpoint<TUser>>();
            
            services.AddSingleton<IAccountEndpoint, PasswordForgotEndpoint<TUser>>();
            services.AddSingleton<IAccountEndpoint, PasswordResetEndpoint<TUser>>();
            services.AddSingleton<IAccountEndpoint, PasswordChangeEndpoint<TUser>>();
            services.AddSingleton<IAccountEndpoint, EmailConfirmationResendEndpoint<TUser>>();
            services.AddSingleton<IAccountEndpoint, EmailConfirmEndpoint<TUser>>();

            if (opts.EnableSignUp)
            {
                services.AddSingleton<IAuthEndpoint, SignUpEndpoint<TUser, SignUpRequest>>();
            }
        }
        
        if (opts.EnableExternalProvidersFlow)
        {
            services.AddSingleton<IAuthEndpoint, ExternalChallengeEndpoint<TUser>>();
            services.AddSingleton<IAuthEndpoint, ExternalProvidersEndpoint<TUser>>();
            services.AddSingleton<IAuthEndpoint, SignInExternalEndpoint<TUser>>();

            if (opts.EnableSignUp)
            {
                services.AddSingleton<IAuthEndpoint, SignUpExternalEndpoint<TUser, SignUpExternalRequest>>();
            }
        }
        
        services.AddSingleton<IAuthEndpoint, SignInRefreshEndpoint<TUser>>();
        services.AddSingleton<IAuthEndpoint, SignOutEndpoint<TUser>>();
        services.AddSingleton<IAuthEndpoint, WhoAmIEndpoint<TUser>>();
        services.AddSingleton<IAccountEndpoint, AccountInfoEndpoint<TUser>>();

        var identityBuilder = services
            .AddIdentityCore<TUser>()
            .AddSignInManager<AufySignInManager<TUser>>()
            .AddRoles<IdentityRole>()
            .AddDefaultTokenProviders();

        var authenticationBuilder = services
            .AddAuthorization()
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme,
                o =>
                {
                    o.Events ??= new JwtBearerEvents();
                    o.Events.OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.TryGetValue(AufyAuthSchemeDefaults.AccessTokenCookieName,
                                out var token))
                        {
                            context.Token = token;
                        }

                        return Task.CompletedTask;
                    };
                    o.ConfigureBearerAuth(opts.JwtBearer);
                })
            .AddScheme<AufyJwtBearerOptions, AufySignInJwtBearerHandler>(
                AufyAuthSchemeDefaults.BearerSignInScheme, _ => { })
            .AddScheme<AufyJwtBearerOptions, AufyTokenJwtBearerHandler>(
                AufyAuthSchemeDefaults.BearerTokenScheme, _ => { })
            .AddJwtBearer(AufyAuthSchemeDefaults.RefreshTokenScheme, o =>
            {
                o.Events ??= new JwtBearerEvents();
                o.Events.OnMessageReceived = context =>
                {
                    if (context.Request.Cookies.TryGetValue(AufyAuthSchemeDefaults.RefreshTokenCookieName,
                            out var token))
                    {
                        context.Token = token;
                    }

                    return Task.CompletedTask;
                };
                o.ConfigureBearerAuth(opts.JwtBearer);
            })
            .AddScheme<PolicySchemeOptions, AufyPolicySignInExternalHandler>(
                AufyAuthSchemeDefaults.SignInExternalPolicyScheme, _ => { })
            .AddCookie(AufyAuthSchemeDefaults.SignInExternalScheme, o =>
            {
                o.Cookie.SameSite = SameSiteMode.None;
                o.Cookie.HttpOnly = true;
                o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                o.ExpireTimeSpan = TimeSpan.FromSeconds(60);
                o.Cookie.Path = opts.AuthApiBasePath + "/signin/external";
                o.Cookie.Name = AufyAuthSchemeDefaults.SignInExternalScheme;
            })
            .AddCookie(AufyAuthSchemeDefaults.SignUpExternalScheme, o =>
            {
                o.Cookie.SameSite = SameSiteMode.None;
                o.Cookie.HttpOnly = true;
                o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                o.ExpireTimeSpan = TimeSpan.FromSeconds(360);
                o.Cookie.Path = opts.AuthApiBasePath + "/signup/external";
                o.Cookie.Name = AufyAuthSchemeDefaults.SignUpExternalScheme;
            });

        return new AufyServiceBuilder<TUser>(services, opts, identityBuilder, authenticationBuilder, configuration);
    }
}