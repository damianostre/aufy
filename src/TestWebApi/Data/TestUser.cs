using Aufy.Core;
using Microsoft.AspNetCore.Identity;

namespace TestWebApi.Data;

public class TestUser : IdentityUser, IAufyUser
{
    public IList<IdentityRole> Roles { get; }
}