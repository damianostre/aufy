using Aufy.Core.AuthSchemes;
using Aufy.Core.Endpoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aufy.Core;

/// <summary>
/// Builder for Aufy services. <br/>
/// Can be used to configure the services and endpoints of Aufy.
/// </summary>
/// <typeparam name="TUser"></typeparam>
public class AufyServiceBuilder<TUser> where TUser : IdentityUser, IAufyUser, new()
{
    public AufyOptions AufyOptions { get; set; }
    public IdentityBuilder IdentityBuilder { get; }
    public AuthenticationBuilder AuthenticationBuilder { get; }
    public IServiceCollection Services { get; private set; }
    public IConfiguration Configuration { get; private set; }

    internal AufyServiceBuilder(
        IServiceCollection services,
        AufyOptions aufyOptions,
        IdentityBuilder identityBuilder,
        AuthenticationBuilder authenticationBuilder,
        IConfiguration configuration)
    {
        Services = services;
        AufyOptions = aufyOptions;
        IdentityBuilder = identityBuilder;
        Configuration = configuration;
        AuthenticationBuilder = authenticationBuilder;
    }

    public AufyServiceBuilder<TUser> UseDefaultAuthScheme(DefaultAuthScheme scheme)
    {
        var schemeName = scheme switch
        {
            DefaultAuthScheme.JwtBearer => JwtBearerDefaults.AuthenticationScheme,
            DefaultAuthScheme.Cookie => CookieAuthenticationDefaults.AuthenticationScheme,
            DefaultAuthScheme.JwtBearerOrCookie => AufyIdentityConstants.CookieAndBearerScheme,
            _ => throw new ArgumentOutOfRangeException(nameof(scheme), scheme, null)
        };

        Services.AddAuthentication(schemeName);

        // Register policy scheme handler only if multi scheme auth is used
        if (scheme == DefaultAuthScheme.JwtBearerOrCookie)
        {
            AuthenticationBuilder.AddScheme<PolicySchemeOptions, AufyBearerOrCookieSchemeHandler>(schemeName, _ => { });
        }

        return this;
    }

    /// <summary>
    /// Registers JWT Bearer authentication schemes
    /// </summary>
    /// <returns>The <see cref="AufyServiceBuilder{TUser}"/>.</returns>
    public AufyServiceBuilder<TUser> AddJwtBearerAuth(Action<JwtBearerOptions>? options = null)
    {
        Services.AddScoped<IRefreshTokenManager, RefreshTokenManager>();
        Services.AddScoped<IJwtTokenService, JwtTokenService>();

        var schemeName = AufyIdentityConstants.BearerScheme;

        AuthenticationBuilder
            .AddJwtBearer(schemeName,
                o =>
                {
                    o.Events ??= new JwtBearerEvents();
                    o.Events.OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.TryGetValue(schemeName,
                                out var token))
                        {
                            context.Token = token;
                        }

                        return Task.CompletedTask;
                    };
                    o.ConfigureBearerAuth(AufyOptions.JwtBearer);
                    options?.Invoke(o);
                })
            .AddScheme<AufyJwtBearerOptions, AufySignInJwtBearerHandler>(
                AufyIdentityConstants.BearerSignInScheme, _ => { })
            .AddJwtBearer(AufyIdentityConstants.RefreshTokenScheme, o =>
            {
                o.Events ??= new JwtBearerEvents();
                o.Events.OnMessageReceived = context =>
                {
                    if (context.Request.Cookies.TryGetValue(AufyIdentityConstants.RefreshTokenScheme,
                            out var token))
                    {
                        context.Token = token;
                    }

                    return Task.CompletedTask;
                };
                o.ConfigureBearerAuth(AufyOptions.JwtBearer);
            });

        // Register JWT token-specific endpoints
        if (AufyOptions.EnableEmailPasswordFlow)
        {
            Services.AddSingleton<IAuthEndpoint, TokenEndpoint<TUser>>();
        }

        Services.AddSingleton<IAuthEndpoint, TokenRefreshEndpoint<TUser>>();
        
        return this;
    }

    /// <summary>
    /// Registers Classic Cookie authentication scheme.
    /// </summary>
    /// <returns>The <see cref="AufyServiceBuilder{TUser}"/>.</returns>
    public AufyServiceBuilder<TUser> AddCookieAuth(Action<CookieAuthenticationOptions>? options = null)
    {
        var schemeName = AufyIdentityConstants.CookieScheme;

        AuthenticationBuilder
            .AddCookie(schemeName, o =>
            {
                o.LoginPath = "/signin";
                o.LogoutPath = "/signout";
                o.AccessDeniedPath = "/";

                options?.Invoke(o);
            });

        return this;
    }

    public AufyServiceBuilder<TUser> ConfigureIdentity(Action<IdentityBuilder> configure)
    {
        configure(IdentityBuilder);
        return this;
    }

    public AufyServiceBuilder<TUser> ConfigureAuthentication(Action<AuthenticationBuilder, AufyOptions> configure)
    {
        configure(AuthenticationBuilder, AufyOptions);
        return this;
    }

    /// <summary>
    /// Adds default CORS policy for the client app.
    /// Uses the base url of the client app from the configuration: Aufy:ClientApp:BaseUrl
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public AufyServiceBuilder<TUser> AddDefaultCorsPolicy()
    {
        var clientAppUrl = AufyOptions.ClientApp.BaseUrl;
        if (string.IsNullOrWhiteSpace(clientAppUrl))
        {
            return this;
        }

        Services.AddCors(opts => opts.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins(clientAppUrl)
                .AllowCredentials()
                .AllowAnyHeader()
                .AllowAnyMethod()
                .WithExposedHeaders("X-Token-Expired");
        }));

        return this;
    }

    /// <summary>
    /// Registers the custom SignUpRequest model for the SignUpEndpoint.
    /// </summary>
    /// <typeparam name="TSignUpRequest"></typeparam>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public AufyServiceBuilder<TUser> UseSignUpModel<TSignUpRequest>() where TSignUpRequest : SignUpRequest
    {
        var descriptor = Services.FirstOrDefault(d => d.ServiceType == typeof(IAuthEndpoint) &&
                                                      d.ImplementationType ==
                                                      typeof(SignUpEndpoint<TUser, SignUpRequest>));
        if (descriptor is null)
        {
            throw new(
                "Error while registering SignUpEndpoint with custom model. Default SignUpEndpoint is not registered");
        }

        Services.Remove(descriptor);
        Services.AddSingleton<IAuthEndpoint, SignUpEndpoint<TUser, TSignUpRequest>>();
        return this;
    }

    /// <summary>
    /// Registers the custom SignUpExternalRequest model for the SignUpExternalEndpoint.
    /// </summary>
    /// <typeparam name="TSignUpExternalRequest"></typeparam>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public AufyServiceBuilder<TUser> UseExternalSignUpModel<TSignUpExternalRequest>()
        where TSignUpExternalRequest : class
    {
        if (AufyOptions.EnableSignUp is false)
        {
            throw new("EnableSignUp is set to false in AufyOptions. Can't register SignUpExternalEndpoint");
        }

        Services.AddSingleton<IAuthEndpoint, SignUpExternalEndpoint<TUser, TSignUpExternalRequest>>();
        AufyOptions.Internal.CustomExternalSignUpFlow = true;
        return this;
    }

    public AufyServiceBuilder<TUser> AddProvider(string provider,
        Action<AuthenticationBuilder, AufyOptions> authBuilder)
    {
        AuthenticationBuilder.AddProviderIfConfigured(provider, AufyOptions,
            b => { authBuilder?.Invoke(b, AufyOptions); });

        return this;
    }
}

public enum DefaultAuthScheme
{
    JwtBearer,
    Cookie,
    JwtBearerOrCookie
}