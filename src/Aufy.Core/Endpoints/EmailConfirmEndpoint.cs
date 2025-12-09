using System.ComponentModel.DataAnnotations;
using System.Text;
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

public class EmailConfirmEndpoint<TUser> : IAccountEndpoint where TUser : IdentityUser, IAufyUser
{
    public RouteHandlerBuilder Map(IEndpointRouteBuilder builder)
    {
        return builder.MapGet("/email/confirm", async Task<Results<Ok, NotFound>> (
                [FromQuery] string? code,
                [FromQuery] string? userId,
                [FromServices] ILogger<EmailConfirmEndpoint<TUser>> logger,
                [FromServices] UserManager<TUser> manager) =>
            {
                if (userId is null || code is null)
                {
                    return TypedResults.NotFound();
                }

                var user = await manager.FindByIdAsync(userId);
                if (user == null)
                {
                    return TypedResults.NotFound();
                }

                if (user.EmailConfirmed)
                {
                    return TypedResults.NotFound();
                }

                var c = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
                var result = await manager.ConfirmEmailAsync(user, c);

                if (result.Succeeded)
                {
                    logger.LogInformation("User: {UserId} confirmed email successfully", user.Id);
                    return TypedResults.Ok();
                }

                logger.LogInformation(
                    "Error confirming email for user with ID {UserId}. Result: {Result}",
                    user.Id,
                    result);
                return TypedResults.NotFound();
            })
            .AllowAnonymous();
    }
}