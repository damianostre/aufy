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

    public async Task<(bool result, string? error)> ShouldUseExternalSignUpFlow(ClaimsIdentity identity)
    {
        var (result, error) = await CheckLogin(identity);
        if (error is not null || result is null)
        {
            _logger.LogError("Error checking login: {Error}", error);
            return (false, error ?? "Error occurred");
        }

        var (_, user) = result;
        if (user is not null)
        {
            _logger.LogInformation("User already exists, should not sign up");
            return (false, null);
        }

        // If custom external signup flow is disabled, we use only sign in endpoint
        if (AufyOptions.Internal.CustomExternalSignUpFlow is false)
        {
            return (false, null);
        }

        //
        if (_options.Value.AutoAccountLinking is false)
        {
            return (false, null);
        }


        var userToLink = await FindUserToLinkLogin(identity);
        if (userToLink is not null)
        {
            return (false, null);
        }

        return (true, null);
    }

    public async Task<(TUser? user, string? error)> LinkLoginAsync(string userId, ClaimsIdentity identity)
    {
        var (checkLoginResult, error) = await CheckLogin(identity);
        if (error is not null || checkLoginResult is null)
        {
            return (null, error ?? "Error occured");
        }

        var (providerKey, userWithLogin) = checkLoginResult;
        if (userWithLogin is not null)
        {
            _logger.LogInformation("Cannot link login, user already exists. User: {UserId}", userWithLogin.Id);
            return (null, null);
        }

        var userToLink = await FindByIdAsync(userId);
        if (userToLink is null)
        {
            return (null, "User not found");
        }

        var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
        var scheme = schemes.FirstOrDefault(x => x.Name == identity.AuthenticationType);
        if (scheme is null)
        {
            return (null, $"Cannot find scheme {identity.AuthenticationType}");
        }

        var result = await AddLoginAsync(
            userToLink, new UserLoginInfo(scheme.Name, providerKey, scheme.DisplayName));
        if (!result.Succeeded)
        {
            _logger.LogError(
                "Failed to add login info to user {UserId}: {Errors}",
                userToLink.Id,
                string.Join(", ", result.Errors.Select(e => e.Description)));
            return (null, "Failed to add login info to user");
        }

        return (userToLink, null);
    }

    public async Task<(TUser? user, string? error)> TryAutoLinkLoginAsync(ClaimsIdentity identity)
    {
        var (checkLoginResult, error) = await CheckLogin(identity);
        if (error is not null || checkLoginResult is null)
        {
            return (null, error ?? "Error occured");
        }

        var (providerKey, user) = checkLoginResult;
        if (user is not null)
        {
            _logger.LogInformation("Cannot link login, user already exists. User: {UserId}", user.Id);
            return (null, null);
        }

        var userToLink = await FindUserToLinkLogin(identity);
        if (userToLink is null)
        {
            return (null, null);
        }

        var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
        var scheme = schemes.FirstOrDefault(x => x.Name == identity.AuthenticationType);
        if (scheme is null)
        {
            return (null, $"Cannot find scheme {identity.AuthenticationType}");
        }

        var result = await AddLoginAsync(
            userToLink, new UserLoginInfo(scheme.Name, providerKey, scheme.DisplayName));
        if (!result.Succeeded)
        {
            _logger.LogError(
                "Failed to add login info to user {UserId}: {Errors}",
                userToLink.Id,
                string.Join(", ", result.Errors.Select(e => e.Description)));
            return (null, "Failed to add login info to user");
        }

        return (userToLink, null);
    }

    private async Task<TUser?> FindUserToLinkLogin(ClaimsIdentity identity)
    {
        var email = identity.FindFirst(ClaimTypes.Email);
        var userToLink = email?.Value is not null ? await FindByEmailAsync(email.Value) : null;
        return userToLink;
    }

    public async Task<(CheckLoginResult<TUser>? result, string? error)> CheckLogin(ClaimsIdentity identity)
    {
        var providerKey = identity.FindFirst(ClaimTypes.NameIdentifier);
        if (providerKey is null)
        {
            return (null, "NameIdentifier claim is missing");
        }

        if (identity.AuthenticationType is null)
        {
            return (null, "AuthenticationType is missing in identity");
        }

        var user = await FindByLoginAsync(identity.AuthenticationType, providerKey.Value);
        if (user is null)
        {
            return (null, null);
        }

        return (new CheckLoginResult<TUser> { User = user, ProviderKey = providerKey.Value }, null);

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
    Task<(bool result, string? error)> ShouldUseExternalSignUpFlow(ClaimsIdentity identity);
}

public record CheckLoginResult<TUser> where TUser : IdentityUser, IAufyUser, new()
{
    public required string ProviderKey { get; set; }
    public TUser? User { get; set; }

    public void Deconstruct(out string providerKey, out TUser? user)
    {
        providerKey = ProviderKey;
        user = User;
    }
}