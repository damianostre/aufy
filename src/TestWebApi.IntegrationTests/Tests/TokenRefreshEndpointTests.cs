using System.Net;
using Aufy.Core.AuthSchemes;
using FluentAssertions;
using Flurl.Http;

namespace TestWebApi.IntegrationTests.Tests;

internal class TokenRefreshEndpointTests : TestBase
{
    [Test]
    public async Task HappyPath()
    {
        var email = TestId.Format("test@test.test");
        var password = "SuperSecret(%2";
        
        await Cli.Given_user_signedUp_correctly(email, password);
        var token = await Cli.Given_user_gets_valid_token(email, password);
        
        var res = await Cli
            .Request("auth","token", "refresh")
            .WithOAuthBearerToken(token.RefreshToken)
            .PostAsync();
        
        await Then_status_code_is(res, HttpStatusCode.OK);
        
        var newToken = await res.GetJsonAsync<AccessTokenResponse>();
        newToken.Should().NotBeNull();
        newToken.AccessToken.Should().NotBeNullOrEmpty();
        newToken.RefreshToken.Should().NotBeNullOrEmpty();
        newToken.AccessToken.Should().NotBe(token.AccessToken);
        newToken.RefreshToken.Should().NotBe(token.RefreshToken);
    }
    
    [Test]
    public async Task InvalidRefreshToken_ShouldReturn401()
    {
        var email = TestId.Format("test@test.test");
        var password = "SuperSecret(%2";
        
        await Cli.Given_user_signedUp_correctly(email, password);
        var token = await Cli.Given_user_gets_valid_token(email, password);
        
        var res = await Cli
            .Request("auth","token", "refresh")
            .WithOAuthBearerToken(token.RefreshToken + "random")
            .PostAsync();
        
        await Then_status_code_is(res, HttpStatusCode.Unauthorized);
    }
}   