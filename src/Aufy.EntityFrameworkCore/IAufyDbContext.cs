using Aufy.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Aufy.EntityFrameworkCore;

public interface IAufyDbContext<TUser> where TUser : IdentityUser
{
    DbSet<AufyRefreshToken> RefreshTokens { get; set; }
}