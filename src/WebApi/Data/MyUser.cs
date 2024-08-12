using System.ComponentModel.DataAnnotations.Schema;
using Aufy.Core;
using Microsoft.AspNetCore.Identity;

namespace WebApi.Data;

public class MyUser: IdentityUser, IAufyUser
{
    [NotMapped]
    public string? AboutMe { get; set; }

    [NotMapped]
    public string? MySiteUrl { get; set; }
}