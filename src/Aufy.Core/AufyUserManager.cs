using System.Security.Claims;
using Aufy.Core.Endpoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aufy.Core;

public class AufyUserManager<TUser> : UserManager<TUser>, IAufyUserManager
    where TUser : IdentityUser, IAufyUser, new()
{
    private readonly SignInManager<TUser> _signInManager;
    private readonly IOptions<AufyOptions> _options;
    private readonly ILogger<AufyUserManager<TUser>> _logger;
    private readonly IServiceProvider _serviceProvider;

    public async Task<bool> UserWithLoginExistsAsync(string provider, string providerKey)
    {
        var user = await FindByLoginAsync(provider, providerKey);
        return user != null;
    }

    public async Task<TUser?> TryLinkLoginAsync(ClaimsPrincipal claimsPrincipal)
    {
        if (_options.Value.AutoAccountLinking is false)
        {
            _logger.LogDebug("Account linking is disabled");
            return null;
        }

        var email = claimsPrincipal.FindFirst(ClaimTypes.Email);
        var user = email?.Value is not null ? await FindByEmailAsync(email.Value) : null;
        if (user is null)
        {
            _logger.LogDebug("Login cannot be linked as no user with email {Email} exists", email?.Value);
            return null;
        }

        var providerKeyClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
        if (providerKeyClaim is null)
        {
            _logger.LogError("NameIdentifier claim is missing");
            throw new("NameIdentifier claim is missing");
        }

        var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
        var scheme = schemes.FirstOrDefault(x => x.Name == claimsPrincipal.Identity?.AuthenticationType);
        if (scheme is null)
        {
            _logger.LogError("Cannot find scheme {Scheme}", claimsPrincipal.Identity?.AuthenticationType);
            throw new("Scheme not found");
        }

        var result = await AddLoginAsync(
            user, new UserLoginInfo(scheme.Name, providerKeyClaim.Value, scheme.DisplayName));
        if (!result.Succeeded)
        {
            _logger.LogError(
                "Failed to add login info to user {UserId}: {Errors}",
                user.Id,
                string.Join(", ", result.Errors.Select(e => e.Description)));
            throw new("Failed to add login info to user");
        }

        return user;
    }

    public async Task<(TUser? user, ProblemHttpResult? problem)> CreateUserWithLoginAsync<TModel>(
        string providerKey,
        HttpContext context,
        TModel req,
        ClaimsPrincipal claimsPrincipal) where TModel : class
    {
        var email = claimsPrincipal.FindFirst(ClaimTypes.Email);
        var name = claimsPrincipal.FindFirst(ClaimTypes.Name);
        var user = new TUser
        {
            UserName = name?.Value,
            Email = email?.Value,
            EmailConfirmed = false,
        };

        var events = _serviceProvider.GetService<ISignUpExternalEndpointEvents<TUser, TModel>>();
        if (events is not null)
        {
            var userCreatingProblem = await events.UserCreatingAsync(req, context.Request, user);
            if (userCreatingProblem is not null)
            {
                return (null, userCreatingProblem);
            }
        }

        var result = await CreateAsync(user);
        if (!result.Succeeded)
        {
            _logger.LogError("Error creating user: {Email}. Result: {Result}", user.Email, result);
            return (null, TypedResults.Problem(result.ToValidationProblem()));
        }

        var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
        var scheme = schemes.FirstOrDefault(x => x.Name == claimsPrincipal.Identity?.AuthenticationType);
        result = await AddLoginAsync(
            user,
            new UserLoginInfo(
                claimsPrincipal.Identity.AuthenticationType,
                providerKey,
                scheme?.DisplayName));

        if (!result.Succeeded)
        {
            _logger.LogError(
                "Failed to add login info to user {UserId}: {Errors}",
                user.Id,
                string.Join(", ", result.Errors.Select(e => e.Description)));

            return (null, TypedResults.Problem("There was an error creating user"));
        }

        result = await AddToRolesAsync(user, _options.Value.DefaultRoles);
        if (!result.Succeeded)
        {
            _logger.LogError("Error adding user to roles: {Result}", result);
            return (null, TypedResults.Problem("Error occured", statusCode: 500));
        }

        if (events is not null)
        {
            await events.UserCreatedAsync(req, context.Request, user);
        }

        return (user, null);
    }

    public AufyUserManager(
        SignInManager<TUser> signInManager,
        IOptions<AufyOptions> options,
        IUserStore<TUser> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<TUser> passwordHasher,
        IEnumerable<IUserValidator<TUser>> userValidators,
        IEnumerable<IPasswordValidator<TUser>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<AufyUserManager<TUser>> logger)
        : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
        _signInManager = signInManager;
        _options = options;
        _logger = logger;
        _serviceProvider = services;
    }
}

public interface IAufyUserManager
{
    Task<bool> UserWithLoginExistsAsync(string provider, string providerKey);
}