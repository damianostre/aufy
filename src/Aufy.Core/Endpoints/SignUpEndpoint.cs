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
                        UserName = req.Email,
                        Email = req.Email,
                    };

                    var events = serviceProvider.GetService<ISignUpEvents<TUser, TModel>>();
                    if (events is not null)
                    {
                        var userCreatingRes = await events.UserCreatingAsync(user, req, httpRequest);
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
                        await events.UserCreatedAsync(user, req, httpRequest);
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
            .AllowAnonymous();
    }
}

public record SignUpRequest
{
    [Required, EmailAddress] public string? Email { get; set; }
    [Required] public string? Password { get; set; }
}

public class SignUpResponse
{

    public bool RequiresEmailConfirmation { get; set; }
}