using Microsoft.AspNetCore.Identity;

namespace Aufy.Core;

public class AufyUserManager<TUser>(
    UserManager<TUser> userManager, SignInManager<TUser> signInManager) : IAufyUserManager
    where TUser : AufyUser
{
    public async Task<bool> UserWithLoginExistsAsync(string provider, string providerKey)
    {
        var user = await userManager.FindByLoginAsync(provider, providerKey);
        return user != null;
    }
}

public interface IAufyUserManager
{
    Task<bool> UserWithLoginExistsAsync(string provider, string providerKey);
}