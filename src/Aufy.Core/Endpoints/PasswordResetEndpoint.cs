using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace Aufy.Core.Endpoints;

public class PasswordResetEndpoint<TUser> : IAccountEndpoint where TUser : AufyUser
{
    public RouteHandlerBuilder Map(IEndpointRouteBuilder builder)
    {
        return builder.MapPost(
                "/password/reset",
                async Task<Results<Ok, BadRequest, ProblemHttpResult>> (
                    [FromBody, Required] PasswordResetRequest req,
                    [FromServices] UserManager<TUser> manager,
                    [FromServices] ILogger<PasswordResetEndpoint<TUser>> logger,
                    [FromServices] IRefreshTokenManager refreshTokenManager) =>
                {
                    ArgumentException.ThrowIfNullOrWhiteSpace(req.Code);
                    ArgumentException.ThrowIfNullOrWhiteSpace(req.Email);
                    ArgumentException.ThrowIfNullOrWhiteSpace(req.Password);
                    
                    var user = await manager.FindByEmailAsync(req.Email);
                    if (user == null)
                    {
                        // Don't reveal that the user does not exist or is not confirmed
                        return TypedResults.BadRequest();
                    }

                    var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(req.Code));
                    var result = await manager.ResetPasswordAsync(user, code, req.Password);
                    if (result.Succeeded)
                    {
                        //clear refresh token on password reset
                        await refreshTokenManager.ClearTokenAsync(user.Id);
                        
                        logger.LogInformation("User: {UserId} reset password successfully", user.Id);
                        return TypedResults.Ok();
                    }

                    logger.LogInformation(
                        "User: {UserId} reset password attempt failed. Result: {Result}",
                        user.Id,
                        result);

                    return TypedResults.Problem(result.ToValidationProblem());
                })
            .AddEndpointFilter<ValidationEndpointFilter<PasswordResetRequest>>()
            .AllowAnonymous();
    }
}

public class PasswordResetRequest
{
    [Required] public string? Code { get; set; }
    [Required, EmailAddress] public string? Email { get; set; }
    [Required] public string? Password { get; set; }
}