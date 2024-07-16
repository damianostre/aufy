using Aufy.Core;
using Microsoft.AspNetCore.Identity;

namespace WebApi.Data;

public class MyUser: IdentityUser, IAufyUser
{
    public string? AboutMe { get; set; }
    public string? MySiteUrl { get; set; }
}