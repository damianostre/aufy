using Microsoft.AspNetCore.Identity;

namespace Aufy.Core;

public interface IAufyUser
{
    IList<IdentityRole> Roles { get; }
}