using Aufy.Core;
using Aufy.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TestWebApi.Data;

public class ApplicationDbContext : AufyDbContext<AufyUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
}