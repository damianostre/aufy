using Aufy.Core.AuthSchemes;
using Aufy.Core.Endpoints;
using Microsoft.AspNetCore.Authentication;
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

        Services.AddCors(opts => opts.AddDefaultPolicy(
            policy =>
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
        var descriptor = Services.FirstOrDefault(
            d => d.ServiceType == typeof(IAuthEndpoint) &&
                 d.ImplementationType == typeof(SignUpEndpoint<TUser, SignUpRequest>));
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
        where TSignUpExternalRequest : SignUpExternalRequest
    {
        var descriptor = Services.FirstOrDefault(
            d => d.ServiceType == typeof(IAuthEndpoint) &&
                 d.ImplementationType == typeof(SignUpExternalEndpoint<TUser, SignUpExternalRequest>));
        if (descriptor is null)
        {
            throw new(
                "Error while registering SignUpExternalEndpoint with custom model. Default SignUpExternalEndpoint is not registered");
        }

        Services.Remove(descriptor);
        Services.AddSingleton<IAuthEndpoint, SignUpExternalEndpoint<TUser, TSignUpExternalRequest>>();
        return this;
    }
    
    public AufyServiceBuilder<TUser> AddProvider(string provider, Action<AuthenticationBuilder, AufyOptions> authBuilder)
    {
        AuthenticationBuilder.AddProviderIfConfigured(provider, AufyOptions, b =>
        {
            authBuilder?.Invoke(b, AufyOptions);
        });

        return this;
    }
}