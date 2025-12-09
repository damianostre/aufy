using System.Net;
using Aufy.Core.AuthSchemes;
using Aufy.Core.Endpoints;
using FluentAssertions;
using Flurl.Http;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace TestWebApi.IntegrationTests.Tests;

internal class PasswordChangeEndpointTests : TestBase
{
    [Test]
    public async Task ValidRequest_WithCookieAuth_ShouldChangePassword()
    {
        // Arrange
        var email = TestId.Format("test@test.test");
        var password = "SuperSecret(%2";
        var newPassword = "NewSuperSecret123!";
        await Cli.Given_user_signedUp_correctly(email, password);
        
        // Sign in to get authentication cookies
        var signInRes = await Cli
            .Request("auth", "signin")
            .PostJsonAsync(new SignInRequest
            {
                Email = email,
                Password = password
            });
        
        signInRes.StatusCode.Should().Be((int)HttpStatusCode.OK);
        signInRes.Cookies.Should().NotBeEmpty();
        
        // Act
        var changePasswordRes = await Cli
            .Request("account", "password", "change")
            .WithCookies(signInRes.Cookies)
            .PostJsonAsync(new ChangePasswordRequest
            {
                Password = password,
                NewPassword = newPassword
            });

        // Assert
        await Then_status_code_is(changePasswordRes, HttpStatusCode.OK);
        
        // Verify we can sign in with the new password
        var newSignInRes = await Cli
            .Request("auth", "signin")
            .PostJsonAsync(new SignInRequest
            {
                Email = email,
                Password = newPassword
            });
        
        newSignInRes.StatusCode.Should().Be((int)HttpStatusCode.OK);
        
        // Verify we cannot sign in with the old password
        var oldPasswordSignInRes = await Cli
            .Request("auth", "signin")
            .PostJsonAsync(new SignInRequest
            {
                Email = email,
                Password = password
            });
        
        oldPasswordSignInRes.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }
    
    [Test]
    public async Task ValidRequest_WithBearerToken_ShouldChangePassword()
    {
        // Arrange
        var email = TestId.Format("test@test.test");
        var password = "SuperSecret(%2";
        var newPassword = "NewSuperSecret123!";
        await Cli.Given_user_signedUp_correctly(email, password);
        
        var token = await Cli.Given_user_gets_valid_token(email, password);
        
        // Act
        var changePasswordRes = await Cli
            .Request("account", "password", "change")
            .WithOAuthBearerToken(token.AccessToken)
            .PostJsonAsync(new ChangePasswordRequest
            {
                Password = password,
                NewPassword = newPassword
            });

        // Assert
        await Then_status_code_is(changePasswordRes, HttpStatusCode.OK);
        
        // Verify we can sign in with the new password
        var newSignInRes = await Cli
            .Request("auth", "signin")
            .PostJsonAsync(new SignInRequest
            {
                Email = email,
                Password = newPassword
            });
        
        newSignInRes.StatusCode.Should().Be((int)HttpStatusCode.OK);
        
        // Verify we cannot sign in with the old password
        var oldPasswordSignInRes = await Cli
            .Request("auth", "signin")
            .PostJsonAsync(new SignInRequest
            {
                Email = email,
                Password = password
            });
        
        oldPasswordSignInRes.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }
    
    [Test]
    public async Task InvalidCurrentPassword_ShouldReturnProblem()
    {
        // Arrange
        var email = TestId.Format("test@test.test");
        var password = "SuperSecret(%2";
        var newPassword = "NewSuperSecret123!";
        await Cli.Given_user_signedUp_correctly(email, password);
        
        var token = await Cli.Given_user_gets_valid_token(email, password);
        
        // Act
        var changePasswordRes = await Cli
            .Request("account", "password", "change")
            .WithOAuthBearerToken(token.AccessToken)
            .PostJsonAsync(new ChangePasswordRequest
            {
                Password = "WrongCurrentPassword123!",
                NewPassword = newPassword
            });

        // Assert
        await Then_status_code_is(changePasswordRes, HttpStatusCode.BadRequest);
        
        // Verify we can still sign in with the original password (password wasn't changed)
        var signInRes = await Cli
            .Request("auth", "signin")
            .PostJsonAsync(new SignInRequest
            {
                Email = email,
                Password = password
            });
        
        signInRes.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }
    
    [Test]
    public async Task MissingPassword_ShouldReturnValidationProblem()
    {
        // Arrange
        var email = TestId.Format("test@test.test");
        var password = "SuperSecret(%2";
        await Cli.Given_user_signedUp_correctly(email, password);
        
        var token = await Cli.Given_user_gets_valid_token(email, password);
        
        // Act
        var changePasswordRes = await Cli
            .Request("account", "password", "change")
            .WithOAuthBearerToken(token.AccessToken)
            .PostJsonAsync(new ChangePasswordRequest
            {
                Password = null, // Missing current password
                NewPassword = "NewPassword123!"
            });

        // Assert
        await Then_status_code_is(changePasswordRes, HttpStatusCode.BadRequest);
    }
    
    [Test]
    public async Task MissingNewPassword_ShouldReturnValidationProblem()
    {
        // Arrange
        var email = TestId.Format("test@test.test");
        var password = "SuperSecret(%2";
        await Cli.Given_user_signedUp_correctly(email, password);
        
        var token = await Cli.Given_user_gets_valid_token(email, password);
        
        // Act
        var changePasswordRes = await Cli
            .Request("account", "password", "change")
            .WithOAuthBearerToken(token.AccessToken)
            .PostJsonAsync(new ChangePasswordRequest
            {
                Password = password,
                NewPassword = null // Missing new password
            });

        // Assert
        await Then_status_code_is(changePasswordRes, HttpStatusCode.BadRequest);
    }
    
    [Test]
    public async Task EmptyPassword_ShouldReturnValidationProblem()
    {
        // Arrange
        var email = TestId.Format("test@test.test");
        var password = "SuperSecret(%2";
        await Cli.Given_user_signedUp_correctly(email, password);
        
        var token = await Cli.Given_user_gets_valid_token(email, password);
        
        // Act
        var changePasswordRes = await Cli
            .Request("account", "password", "change")
            .WithOAuthBearerToken(token.AccessToken)
            .PostJsonAsync(new ChangePasswordRequest
            {
                Password = "", // Empty current password
                NewPassword = "NewPassword123!"
            });

        // Assert
        await Then_status_code_is(changePasswordRes, HttpStatusCode.BadRequest);
    }
    
    [Test]
    public async Task EmptyNewPassword_ShouldReturnValidationProblem()
    {
        // Arrange
        var email = TestId.Format("test@test.test");
        var password = "SuperSecret(%2";
        await Cli.Given_user_signedUp_correctly(email, password);
        
        var token = await Cli.Given_user_gets_valid_token(email, password);
        
        // Act
        var changePasswordRes = await Cli
            .Request("account", "password", "change")
            .WithOAuthBearerToken(token.AccessToken)
            .PostJsonAsync(new ChangePasswordRequest
            {
                Password = password,
                NewPassword = "" // Empty new password
            });

        // Assert
        await Then_status_code_is(changePasswordRes, HttpStatusCode.BadRequest);
    }
    
    [Test]
    public async Task WeakNewPassword_ShouldReturnValidationProblem()
    {
        // Arrange
        var email = TestId.Format("test@test.test");
        var password = "SuperSecret(%2";
        await Cli.Given_user_signedUp_correctly(email, password);
        
        var token = await Cli.Given_user_gets_valid_token(email, password);
        
        // Act
        var changePasswordRes = await Cli
            .Request("account", "password", "change")
            .WithOAuthBearerToken(token.AccessToken)
            .PostJsonAsync(new ChangePasswordRequest
            {
                Password = password,
                NewPassword = "123" // Too weak/short password
            });

        // Assert
        await Then_status_code_is(changePasswordRes, HttpStatusCode.BadRequest);
        
        // Verify we can still sign in with the original password (password wasn't changed)
        var signInRes = await Cli
            .Request("auth", "signin")
            .PostJsonAsync(new SignInRequest
            {
                Email = email,
                Password = password
            });
        
        signInRes.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }
    
    [Test]
    public async Task SamePasswordAsNew_ShouldSucceed()
    {
        // Arrange
        var email = TestId.Format("test@test.test");
        var password = "SuperSecret(%2";
        await Cli.Given_user_signedUp_correctly(email, password);
        
        var token = await Cli.Given_user_gets_valid_token(email, password);
        
        // Act
        var changePasswordRes = await Cli
            .Request("account", "password", "change")
            .WithOAuthBearerToken(token.AccessToken)
            .PostJsonAsync(new ChangePasswordRequest
            {
                Password = password,
                NewPassword = password // Same as current password
            });

        // Assert - ASP.NET Identity allows changing to the same password
        await Then_status_code_is(changePasswordRes, HttpStatusCode.OK);
        
        // Verify we can still sign in with the password
        var signInRes = await Cli
            .Request("auth", "signin")
            .PostJsonAsync(new SignInRequest
            {
                Email = email,
                Password = password
            });
        
        signInRes.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }
}
