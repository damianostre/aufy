using System.Net;
using Aufy.Core.AuthSchemes;
using Aufy.Core.Endpoints;
using FluentAssertions;
using Flurl.Http;

namespace TestWebApi.IntegrationTests.Tests;

internal class SignInEndpointTests : TestBase
{
    [Test]
    public async Task ValidRequest_WithToken_ShouldReturnAccessToken()
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
    }
}