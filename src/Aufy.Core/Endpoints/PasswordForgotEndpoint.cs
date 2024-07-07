using System.ComponentModel.DataAnnotations;
using System.Text;
using Aufy.Core.EmailSender;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aufy.Core.Endpoints;

public class PasswordForgotEndpoint<TUser> : IAccountEndpoint where TUser : AufyUser
{
    public RouteHandlerBuilder Map(IEndpointRouteBuilder builder)
    {
        return builder.MapPost("/password/forgot", async (
                [FromBody, Required] PasswordForgotRequest req,
                [FromServices] UserManager<TUser> manager,
                [FromServices] IAufyEmailSenderManager<TUser> emailSender,
                [FromServices] IOptions<IdentityOptions> identityOptions,
                [FromServices] IOptions<AufyOptions> options,
                [FromServices] ILogger<PasswordForgotEndpoint<TUser>> logger,
                HttpRequest httpRequest) =>
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(req.Email, nameof(req.Email));

                var user = await manager.FindByEmailAsync(req.Email);
                if (user == null)
                {
                    logger.LogInformation("User: {Email} not found", req.Email);
                    //Don't leak information about the user
                    return TypedResults.Ok();
                }

                if (identityOptions.Value.SignIn.RequireConfirmedEmail && user is not {EmailConfirmed: true})
                {
                    //Don't leak information about the user
                    return TypedResults.Ok();
                }

                var code = await manager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var baseUri = new Uri(
                    new Uri(options.Value.ClientApp.BaseUrl ?? $"{httpRequest.Scheme}://{httpRequest.Host}"),
                    options.Value.ClientApp.PasswordResetPath);
                var link = new Uri(baseUri, $"?code={code}");
                await emailSender.SendPasswordResetAsync(user, link.ToString());

                return TypedResults.Ok();
            })
            .AddEndpointFilter<ValidationEndpointFilter<PasswordForgotRequest>>()
            .AllowAnonymous();
    }
}

public class PasswordForgotRequest
{
    [Required, EmailAddress] public string? Email { get; set; }
}