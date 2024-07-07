using System.ComponentModel.DataAnnotations;
using AspNet.Security.OAuth.Discord;
using AspNet.Security.OAuth.GitHub;
using WebApi.Data;
using Aufy.Core;
using Aufy.Core.AuthSchemes;
using Aufy.Core.Endpoints;
using Aufy.EntityFrameworkCore;
using Aufy.FluentEmail;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(hostingContext.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services
    .AddAufy<AufyUser>(builder.Configuration)
    .AddProvider(GitHubAuthenticationDefaults.AuthenticationScheme, (auth, options) =>
    {
        auth.AddGitHub(o => o.Configure(GitHubAuthenticationDefaults.AuthenticationScheme, options));
    })
    .AddProvider(DiscordAuthenticationDefaults.AuthenticationScheme, (auth, options) =>
    {
        auth.AddDiscord(o => o.Configure(DiscordAuthenticationDefaults.AuthenticationScheme, options));
    })
    .AddDefaultCorsPolicy()
    .AddEntityFrameworkStore<ApplicationDbContext, AufyUser>()
    .AddFluentEmail();
    // .UseSignUpModel<MySignUpRequest>()
    // .UseExternalSignUpModel<MySignUpExternalRequest>();


builder.Services.Configure<IdentityOptions>(options => { options.SignIn.RequireConfirmedEmail = true; });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseMigrationsEndPoint();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapAufyEndpoints(c =>
{
    c.ConfigureAuthRouteGroup(routeGroupBuilder => { routeGroupBuilder.WithDescription("CUSTOM DESC - Auth routes"); });
    c.ConfigureRoute<SignUpEndpoint<AufyUser, SignUpRequest>>(r =>
        r.WithDescription("CUSTOM DESC - Sign up a new user with email and password"));
});

app.MapFallbackToFile("index.html");
app.Run();