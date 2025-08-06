using System.Net;
using Aufy.Core.AuthSchemes;
using Aufy.Core.Endpoints;
using FluentAssertions;
using Flurl.Http;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace TestWebApi.IntegrationTests.Tests;

internal class SignInEndpointTests : TestBase
{
    [Test]
    public async Task ValidRequest_ShouldSignInWithCookies()
    {
        // Arrange
        var email = TestId.Format("test@test.test");
        var password = "SuperSecret(%2";
        await Cli.Given_user_signedUp_correctly(email, password);
        
        // Act
        var signInRes = await Cli
            .Request("auth", "signin")
            .PostJsonAsync(new SignInRequest
            {
                Email = email,
                Password = password
            });

        // Assert
        await Then_status_code_is(signInRes, HttpStatusCode.OK);

        // Should set authentication cookies (classic cookie auth)
        signInRes.Cookies.Should().NotBeEmpty();
        signInRes.Cookies.Should().Contain(c => c.Name == $"{CookieAuthenticationDefaults.CookiePrefix}{AufyIdentityConstants.CookieScheme}");
        
        // Verify we can access protected endpoints with the authentication cookies
        var response = await Cli
            .Request("auth", "whoami")
            .WithCookies(signInRes.Cookies)
            .GetAsync();
        
        await Then_status_code_is(response, HttpStatusCode.OK);
    }
    
    [Test]
    public async Task InvalidCredentials_ShouldReturnProblem()
    {
        // Arrange
        var email = TestId.Format("test@test.test");
        var password = "SuperSecret(%2";
        await Cli.Given_user_signedUp_correctly(email, password);
        
        // Act
        var signInRes = await Cli
            .Request("auth", "signin")
            .PostJsonAsync(new SignInRequest
            {
                Email = email,
                Password = "WrongPassword123!"
            });

        // Assert
        await Then_status_code_is(signInRes, HttpStatusCode.BadRequest);
        
        // Should not set any authentication cookies on failure
        signInRes.Cookies.Should().BeEmpty();
    }
    
    [Test]
    public async Task NonExistentUser_ShouldReturnProblem()
    {
        // Act
        var signInRes = await Cli
            .Request("auth", "signin")
            .PostJsonAsync(new SignInRequest
            {
                Email = TestId.Format("nonexistent@test.test"),
                Password = "SomePassword123!"
            });

        // Assert
        await Then_status_code_is(signInRes, HttpStatusCode.BadRequest);
        
        // Should not set any authentication cookies on failure
        signInRes.Cookies.Should().BeEmpty();
    }
}