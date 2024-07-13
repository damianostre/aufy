using System.Net;
using Aufy.Core.AuthSchemes;
using Aufy.Core.Endpoints;
using FluentAssertions;
using Flurl.Http;

namespace TestWebApi.IntegrationTests.Tests;

internal class SignInEndpointTests : TestBase
{
    [Test]
    public async Task ValidRequest_ShouldReturnAccessToken()
    {
        // Arrange
        var email = TestId.Format("test@test.test");
        var password = "SuperSecret(%2";
        await Cli.Given_user_signedUp_correctly(email, password);
        
        // Act
        var tokenRes = await Cli
            .Request("auth", "signin")
            .PostJsonAsync(new SignInRequest
            {
                Email = email,
                Password = password
            });

        await Then_status_code_is(tokenRes, HttpStatusCode.OK);

        var token = await tokenRes.GetJsonAsync<AccessTokenResponse>();
        token.Should().NotBeNull();
        token.AccessToken.Should().NotBeNullOrEmpty();
        
        tokenRes.Cookies.Should().NotBeEmpty();
        tokenRes.Cookies.Should().Contain(c => c.Name == AufyAuthSchemeDefaults.RefreshTokenCookieName);
        tokenRes.Cookies.Should().NotContain(c => c.Name == ".AspNetCore.Cookies");
        
        var response = await Cli
            .Request("auth", "whoami")
            .WithOAuthBearerToken(token.AccessToken)
            .GetAsync();
        
        await Then_status_code_is(response, HttpStatusCode.OK);
    }
    
    [Test]
    public async Task ValidRequest_UseCookie_ShouldReturnAccessTokenInCookie()
    {
        // Arrange
        var email = TestId.Format("test@test.test");
        var password = "SuperSecret(%2";
        await Cli.Given_user_signedUp_correctly(email, password);
        
        // Act
        var tokenRes = await Cli
            .Request("auth", "signin")
            .SetQueryParam("usecookie", true)
            .PostJsonAsync(new SignInRequest
            {
                Email = email,
                Password = password
            });

        await Then_status_code_is(tokenRes, HttpStatusCode.OK);

        var token = await tokenRes.GetJsonAsync<AccessTokenResponse>();
        token.Should().NotBeNull();
        token.AccessToken.Should().BeNull();
        token.RefreshToken.Should().BeNull();
        
        tokenRes.Cookies.Should().NotBeEmpty();
        tokenRes.Cookies.Should().Contain(c => c.Name == AufyAuthSchemeDefaults.RefreshTokenCookieName);
        tokenRes.Cookies.Should().Contain(c => c.Name == AufyAuthSchemeDefaults.AccessTokenCookieName);
        tokenRes.Cookies.Should().NotContain(c => c.Name == ".AspNetCore.Cookies");
        
        var response = await Cli
            .Request("auth", "whoami")
            .WithCookies(tokenRes.Cookies)
            .GetAsync();
        
        await Then_status_code_is(response, HttpStatusCode.OK);
        

    }
}