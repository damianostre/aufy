using System.Security.Claims;
using Aufy.Core.AuthSchemes;
using Aufy.Core.Endpoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aufy.Core;

public class AufySignInManager<TUser>(
    AufyUserManager<TUser> userManager,
    IHttpContextAccessor contextAccessor,
    IUserClaimsPrincipalFactory<TUser> claimsFactory,
    IOptions<IdentityOptions> optionsAccessor,
    ILogger<SignInManager<TUser>> logger,
    IAuthenticationSchemeProvider schemes,
    IUserConfirmation<TUser> confirmation,
    IOptions<AufyOptions> options,
    IServiceProvider serviceProvider)
    : SignInManager<TUser>(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    where TUser : IdentityUser, IAufyUser, new()
{
    // It's not possible to pass AuthenticationProperties directly to PasswordSignInAsync, so we store it here and use it later.
    private Dictionary<string, object?>? _authenticationParameters = null;
    
    public override Task SignInWithClaimsAsync(
        TUser user, 
        AuthenticationProperties? authenticationProperties,
        IEnumerable<Claim> additionalClaims)
    {
        if (_authenticationParameters is not null)
        {
            authenticationProperties ??= new AuthenticationProperties();
            foreach (var kvp in _authenticationParameters)
            {
                authenticationProperties.Parameters[kvp.Key] = kvp.Value;
            }
        }
        
        return base.SignInWithClaimsAsync(user, authenticationProperties, additionalClaims);
    }

    public async Task SignInWith(
        string signInScheme,
        TUser user,
        AuthenticationProperties properties,
        string authenticationMethod)
    {
        AuthenticationScheme = signInScheme;
        await SignInAsync(user, properties, authenticationMethod);
    }

    public async Task<(TUser? user, ProblemHttpResult? problem)> HandleExternalAuthAsync<TModel>(
        ClaimsPrincipal claimsPrincipal,
        TModel signUpModel) where TModel : class
    {
        var context = contextAccessor.HttpContext;
        if (context is null)
        {
            throw new InvalidOperationException("HttpContext is not available");
        }
        
        await context.SignOutAsync(AufyIdentityConstants.ExternalScheme);
        await context.SignOutAsync(AufyIdentityConstants.ExternalSignUpScheme);

        if (claimsPrincipal.Identity?.AuthenticationType is null)
        {
            Logger.LogInformation("User has no authentication type");
            return (null, TypedResults.Problem("Authentication type is missing"));
        }

        var (checkLoginResult, checkError) = await userManager.CheckLogin(claimsPrincipal.Identity as ClaimsIdentity);
        if (checkError is not null || checkLoginResult is null)
        {
            Logger.LogError("Error checking login: {Error}", checkError);
            return (null, TypedResults.Problem("Error occurred"));
        }

        var (providerKey, existingUser) = checkLoginResult;
        if (existingUser is not null)
        {
            return (existingUser, null);
        }
        
        if (!options.Value.EnableSignUp)
        {
            return (null, TypedResults.Problem("Sign up is disabled"));
        }

        if (options.Value.AutoAccountLinking)
        {
            var (linkedUser, linkError) = await userManager.TryAutoLinkLoginAsync(claimsPrincipal.Identity as ClaimsIdentity);
            if (linkError is not null)
            {
                logger.LogError("Error linking login: {Error}", linkError);
                return (null, TypedResults.Problem("Error occurred"));
            }
            
            if (linkedUser is not null)
            {
                return (linkedUser, null);
            }
        }

        var (newUser, createProblem) = await userManager.CreateUserWithLoginAsync(providerKey, context, signUpModel, claimsPrincipal);
        if (createProblem is not null)
        {
            return (null, createProblem);
        }

        if (newUser is null)
        {
            logger.LogError("Failed to create user");
            return (null, TypedResults.Problem("Error occurred"));
        }

        return (newUser, null);
    }

    public async Task<SignInResult> SignInWithPasswordAsync(
        string email,
        string password,
        string authenticationScheme,
        bool isPersistent = true,
        Dictionary<string, object?>? authenticationParameters = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        ArgumentException.ThrowIfNullOrWhiteSpace(authenticationScheme);

        _authenticationParameters = authenticationParameters;
        var context = contextAccessor.HttpContext;
        if (context is null)
        {
            throw new InvalidOperationException("HttpContext is not available");
        }

        var events = serviceProvider.GetService<ISignInEndpointEvents<TUser>>();

        var user = await UserManager.FindByEmailAsync(email);
        if (user == null)
        {
            if (events is not null && context is not null)
            {
                var signInRequest = new SignInRequest { Email = email, Password = password };
                await events.UserNotFound(signInRequest, context);
            }
            
            Logger.LogInformation("User {Email} failed to sign in. Reason: User not found", email);
            return SignInResult.Failed;
        }

        AuthenticationScheme = authenticationScheme;
        var result = await PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure: true);
        
        if (!result.Succeeded)
        {
            if (events is not null && context is not null)
            {
                var signInRequest = new SignInRequest { Email = email, Password = password };
                await events.SignInFailedAsync(signInRequest, context, result);
            }
            
            Logger.LogInformation("User {Email} failed to sign in. Result: {Result}", email, result);
            return result;
        }
        
        if (events is not null && context is not null)
        {
            await events.SignInSucceededAsync(user, context);
        }
        
        Logger.LogInformation("User {Email} signed in", email);
        return result;
    }
}