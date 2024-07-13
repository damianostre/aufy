using System.Net;
using Aufy.Core.AuthSchemes;
using FluentAssertions;
using Flurl.Http;

namespace TestWebApi.IntegrationTests.Tests;

internal class SignInRefreshEndpointTests : TestBase
{
    [Test]
    public async Task HappyPath()
    {
        //Arrange
        var email = TestId.Format("test@test.test");
        var password = "SuperSecret(%2";
        
        await Cli.Given_user_signedUp_correctly(email, password);
        var (accessToken, refreshToken) = await Cli.Given_user_signin(email, password);
        
        var cookieJar = new CookieJar();
        refreshToken.Secure = false; // Integration tests run on http
        cookieJar.AddOrReplace(refreshToken);
        
        //Act
        var res = await Cli
            .Request("auth", "signin", "refresh")
            .WithCookies(cookieJar)
            .PostAsync();
        
        //Assert
        await Then_status_code_is(res, HttpStatusCode.OK);

        var newToken = await res.GetJsonAsync<AccessTokenResponse>();
        newToken.Should().NotBeNull();
        
        res.Cookies.Should().NotBeEmpty();
        var newRefreshToken = res.Cookies.FirstOrDefault(c => c.Name == AufyAuthSchemeDefaults.RefreshTokenCookieName)?.Value;
        
        newToken.AccessToken.Should().NotBeNullOrEmpty();
        newRefreshToken.Should().NotBeNullOrEmpty();
        accessToken.Should().NotBe(newToken.AccessToken);
        newRefreshToken.Should().NotBe(refreshToken.Value);
    }
    
    [Test]
    public async Task InvalidRefreshToken_ShouldReturn401()
    {
        //Arrange
        var email = TestId.Format("test@test.test");
        var password = "SuperSecret(%2";
        
        await Cli.Given_user_signedUp_correctly(email, password);
        var (_, refreshToken) = await Cli.Given_user_signin(email, password);
        
        //Act
        var res = await Cli
            .Request("auth","token", "refresh")
            .PostAsync();
        
        //Assert
        await Then_status_code_is(res, HttpStatusCode.Unauthorized);

    }
    [Test]
    public async Task HappyPath_UseCookie()
    {
        //Arrange
        var email = TestId.Format("test@test.test");
        var password = "SuperSecret(%2";
        
        await Cli.Given_user_signedUp_correctly(email, password);
        var (accessToken, refreshToken) = await Cli.Given_user_signin(email, password);
        
        var cookieJar = new CookieJar();
        refreshToken.Secure = false; // Integration tests run on http
        cookieJar.AddOrReplace(refreshToken);
        
        //Act
        var res = await Cli
            .Request("auth", "signin", "refresh")
            .SetQueryParam("usecookie", true)
            .WithCookies(cookieJar)
            .PostAsync();
        
        //Assert
        await Then_status_code_is(res, HttpStatusCode.OK);
        
        res.Cookies.Should().NotBeEmpty();
        var newRefreshToken = res.Cookies.FirstOrDefault(c => c.Name == AufyAuthSchemeDefaults.RefreshTokenCookieName)?.Value;
        var newToken = res.Cookies.FirstOrDefault(c => c.Name == AufyAuthSchemeDefaults.AccessTokenCookieName)?.Value;
        
        newToken.Should().NotBeNullOrEmpty();
        newRefreshToken.Should().NotBeNullOrEmpty();
        accessToken.Should().NotBe(newToken);
        newRefreshToken.Should().NotBe(refreshToken.Value);
    }
    
}