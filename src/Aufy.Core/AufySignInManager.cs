using System.Security.Claims;
using Aufy.Core.AuthSchemes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aufy.Core;

public class AufySignInManager<TUser> : SignInManager<TUser> where TUser : class, IAufyUser
{
    public bool UseCookie { get; set; }
    
    public AufySignInManager(
        UserManager<TUser> userManager, 
        IHttpContextAccessor contextAccessor, 
        IUserClaimsPrincipalFactory<TUser> claimsFactory, 
        IOptions<IdentityOptions> optionsAccessor, 
        ILogger<SignInManager<TUser>> logger, 
        IAuthenticationSchemeProvider schemes, 
        IUserConfirmation<TUser> confirmation) 
        : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    {
       AuthenticationScheme = AufyAuthSchemeDefaults.BearerSignInScheme;
    }

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
}