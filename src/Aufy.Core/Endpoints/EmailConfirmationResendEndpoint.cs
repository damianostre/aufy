using System.ComponentModel.DataAnnotations;
using System.Text;
using Aufy.Core.EmailSender;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace Aufy.Core.Endpoints;

public class EmailConfirmationResendEndpoint<TUser> : IAccountEndpoint where TUser : AufyUser
{
    public RouteHandlerBuilder Map(IEndpointRouteBuilder builder)
    {
        return builder.MapPost(
            "/email/confirm/resend", 
             async ([FromBody, Required] EmailConfirmationResendRequest req, 
                 [FromServices] UserManager<TUser> manager, 
                 [FromServices] IAufyEmailSenderManager<TUser> emailSender,
                 [FromServices] IOptions<AufyOptions> options,
                 HttpRequest httpRequest) =>
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(req.Email, nameof(req.Email));
                
                var user = await manager.FindByEmailAsync(req.Email);
                if (user == null)
                {
                    return TypedResults.Ok();
                }
                
                if (user.EmailConfirmed)
                {
                    return TypedResults.Ok();
                }

                var code = await manager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            
                var baseUri = new Uri(
                    new Uri(options.Value.ClientApp.BaseUrl ?? $"{httpRequest.Scheme}://{httpRequest.Host}"), 
                    options.Value.ClientApp.EmailConfirmationPath);
                var link = new Uri(baseUri, $"?code={code}&userId={user.Id}");
                await emailSender.SendEmailConfirmationAsync(user, link.ToString());

                return TypedResults.Ok();
            })
            .AddEndpointFilter<ValidationEndpointFilter<EmailConfirmationResendRequest>>()
            .AllowAnonymous();
    }
}

public class EmailConfirmationResendRequest 
{
    [Required, EmailAddress]
    public string? Email { get; set; }
}
