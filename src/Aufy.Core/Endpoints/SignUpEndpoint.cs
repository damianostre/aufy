using System.ComponentModel.DataAnnotations;
using System.Text;
using Aufy.Core.EmailSender;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aufy.Core.Endpoints;

public class SignUpEndpoint<TUser, TModel> : IAuthEndpoint where TModel : SignUpRequest where TUser : IdentityUser, IAufyUser, new()
{
    public RouteHandlerBuilder Map(IEndpointRouteBuilder builder)
    {
        return builder.MapPost("/signup",
                async Task<Results<Ok<SignUpResponse>, BadRequest, ProblemHttpResult>>
                ([FromBody] TModel req,
                    [FromServices] UserManager<TUser> manager,
                    [FromServices] ILogger<SignUpEndpoint<TUser, TModel>> logger,
                    [FromServices] IAufyEmailSenderManager<TUser> emailSender,
                    [FromServices] IOptions<AufyOptions> options,
                    [FromServices] IServiceProvider serviceProvider,
                    [FromServices] IOptions<IdentityOptions> identityOptions,
                    HttpRequest httpRequest) =>
                {
                    ArgumentException.ThrowIfNullOrWhiteSpace(req.Email, nameof(req.Email));
                    ArgumentException.ThrowIfNullOrWhiteSpace(req.Password, nameof(req.Password));
                    
                    var user = await manager.FindByEmailAsync(req.Email);
                    if (user != null)
                    {
                        return TypedResults.Problem("Account with this email already exists", statusCode: 400);
                    }

                    user = new TUser
                    {
                        Email = req.Email,
                        UserName = req.Email
                    };

                    var events = serviceProvider.GetService<ISignUpEndpointEvents<TUser, TModel>>();
                    if (events is not null)
                    {
                        var userCreatingRes = await events.UserCreatingAsync(req, httpRequest, user);
                        if (userCreatingRes is not null)
                        {
                            return userCreatingRes;
                        }
                    }

                    var result = await manager.CreateAsync(user, req.Password);
                    if (!result.Succeeded)
                    {
                        logger.LogError("Error creating user: {Email}. Result: {Result}", req.Email, result);
                        return TypedResults.Problem(result.ToValidationProblem());
                    }

                    result = await manager.AddToRolesAsync(user, options.Value.DefaultRoles);
                    if (!result.Succeeded)
                    {
                        logger.LogError("Error adding user to roles: {Result}", result);
                        return TypedResults.Problem("Error occured", statusCode: 500);
                    }

                    if (events is not null)
                    {
                        await events.UserCreatedAsync(req, httpRequest, user);
                    }

                    logger.LogInformation("User: {Email} created a new account with password", req.Email);

                    var code = await manager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    var baseUri = new Uri(
                        new Uri(options.Value.ClientApp.BaseUrl ?? $"{httpRequest.Scheme}://{httpRequest.Host}"),
                        options.Value.ClientApp.EmailConfirmationPath);
                    var link = new Uri(baseUri, $"?code={code}&userId={user.Id}");
                    await emailSender.SendEmailConfirmationAsync(user, link.ToString());

                    return TypedResults.Ok(new SignUpResponse
                    {
                        RequiresEmailConfirmation = identityOptions.Value.SignIn.RequireConfirmedEmail
                    });
                })
            .AddEndpointFilter<ValidationEndpointFilter<TModel>>()
            .AllowAnonymous();
    }
}

/// <summary>
/// Extension point for the SignUpEndpoint.
/// </summary>
/// <typeparam name="TUser"></typeparam>
/// <typeparam name="TModel"></typeparam>
public interface ISignUpEndpointEvents<in TUser, in TModel> where TModel : SignUpRequest where TUser : IAufyUser
{
    /// <summary>
    /// Called when a user is being created. <br/>
    /// Return a ProblemHttpResult if the user can't be created.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="httpRequest"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    Task<ProblemHttpResult?> UserCreatingAsync(TModel model, HttpRequest httpRequest, TUser user)
    {
        return Task.FromResult<ProblemHttpResult?>(null);
    }
    
    /// <summary>
    /// Called when a user is created and saved to the database.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="httpRequest"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    Task UserCreatedAsync(TModel model, HttpRequest httpRequest, TUser user)
    {
        return Task.CompletedTask;
    }
}

public class SignUpRequest
{
    [Required, EmailAddress] public string? Email { get; set; }
    [Required] public string? Password { get; set; }
}

public class SignUpResponse
{
    public bool RequiresEmailConfirmation { get; set; }
}