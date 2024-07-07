using Aufy.Core;
using Aufy.Core.EmailSender;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using TestWebApi.Data;

namespace TestWebApi.IntegrationTests;

public class MyWebApplicationFactory : WebApplicationFactory<Program>
{
    public static Lazy<MyWebApplicationFactory> Instance => new(() => new MyWebApplicationFactory());
    public MyWebApplicationFactory()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<AufyOptions>>();
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();

        foreach (var role in options.Value.DefaultRoles)
        {
            dbContext.Roles.Add(new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = role,
                NormalizedName = role.ToUpper()
            });
        }

        dbContext.SaveChanges();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Debug()
            .CreateLogger();

        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            services.AddScoped<IAufyEmailSenderManager<AufyUser>, TestAufyEmailSenderManager>();
            services.Configure<AufyOptions>(o =>
            {
                o.ClientApp.BaseUrl = null;
                //set to endpoint base path
                o.ClientApp.EmailConfirmationPath = "api/account/email/confirm";
            });

            services.Configure<IdentityOptions>(options => { options.SignIn.RequireConfirmedEmail = false; });
        });
    }

    public static async Task DisposeInstance()
    {
        await Instance.Value.DisposeAsync();
    }
}