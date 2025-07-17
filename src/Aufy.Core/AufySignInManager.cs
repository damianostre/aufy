using System.Security.Claims;
using Aufy.Core.AuthSchemes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
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
    IOptions<AufyOptions> options)
    : SignInManager<TUser>(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    where TUser : IdentityUser, IAufyUser, new()
{
    public override Task SignInWithClaimsAsync(
        TUser user, AuthenticationProperties? authenticationProperties, IEnumerable<Claim> additionalClaims)
    {
        var properties = authenticationProperties ?? new AuthenticationProperties();
        if (UseCookie)
        {
            properties.SetParameter("useCookie", true);
        }
        
        return base.SignInWithClaimsAsync(user, properties, additionalClaims);
    }
    
    public async Task<(TUser? user, ProblemHttpResult? problem)> HandleExternalAuthAsync<TModel>(
        ClaimsPrincipal claimsPrincipal,
        HttpContext context,
        TModel signUpModel) where TModel : class
    {
        await context.SignOutAsync(AufyAuthSchemeDefaults.SignInExternalScheme);
        await context.SignOutAsync(AufyAuthSchemeDefaults.SignUpExternalScheme);

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
}