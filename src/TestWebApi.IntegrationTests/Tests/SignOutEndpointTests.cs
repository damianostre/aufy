using System.Net;
using Flurl.Http;

namespace TestWebApi.IntegrationTests.Tests;

internal class SignOutEndpointTests : TestBase
{
    [Test]
    public async Task CallSignOut_NoAuth_ShouldReturn401()
    {
        // Arrange
        // Act
        var res = await Cli
            .Request("auth", "signout")
            .PostAsync();

        // Assert
        await Then_status_code_is(res, HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task CallSignOut_WithToken_ShouldReturn200_AndInvalidateRefreshToken()
    {
        // Arrange
        var email = TestId.Format("test@test.test");
        var password = "SuperSecret(%2";

        await Cli.Given_user_signedUp_correctly(email, password);
        var token = await Cli.Given_user_gets_valid_token(email, password);

        // Act: sign out with access token
        var signOutRes = await Cli
            .Request("auth", "signout")
            .WithOAuthBearerToken(token.AccessToken)
            .PostAsync();

        // Assert
        await Then_status_code_is(signOutRes, HttpStatusCode.OK);

        // Try to refresh with the old refresh token — should be invalidated by sign out
        var refreshAttempt = await Cli
            .Request("auth", "token", "refresh")
            .WithOAuthBearerToken(token.RefreshToken)
            .PostAsync();

        await Then_status_code_is(refreshAttempt, HttpStatusCode.Unauthorized);
    }
}
