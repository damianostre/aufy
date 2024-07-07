using System.Net;
using Flurl.Http;

namespace TestWebApi.IntegrationTests.Tests;

internal class WhoAmIEndpointTests : TestBase
{
    [Test]
    public async Task CallWhoAmI_NoAuth_ShouldReturn401()
    {
        // Arrange
        // Act
        var response = await Cli
            .Request("auth", "whoami")
            .GetAsync();

        // Assert
        await Then_status_code_is(response, HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task CallWhoAmI_WithToken_ShouldReturnAccessToken()
    {
        // Arrange
        var email = TestId.Format("test@test.test");
        var password = "SuperSecret(%2";

        await Cli.Given_user_signedUp_correctly(email, password);
        var token = await Cli.Given_user_gets_valid_token(email, password);

        // Act
        var response = await Cli
            .Request("auth", "whoami")
            .WithOAuthBearerToken(token.AccessToken)
            .GetAsync();

        // Assert
        await Then_status_code_is(response, HttpStatusCode.OK);
    }
}