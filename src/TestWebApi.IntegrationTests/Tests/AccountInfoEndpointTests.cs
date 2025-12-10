using System.Net;
using Aufy.Core.Endpoints;
using FluentAssertions;
using Flurl.Http;

namespace TestWebApi.IntegrationTests.Tests;

internal class AccountInfoEndpointTests : TestBase
{
    [Test]
    public async Task CallAccountInfo_NoAuth_ShouldReturn401()
    {
        // Arrange
        // Act
        var res = await Cli
            .Request("account", "info")
            .GetAsync();

        // Assert
        await Then_status_code_is(res, HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task CallAccountInfo_WithToken_ShouldReturnAccountInfo()
    {
        // Arrange
        var email = TestId.Format("test@test.test");
        var password = "SuperSecret(%2";

        await Cli.Given_user_signedUp_correctly(email, password);
        var token = await Cli.Given_user_gets_valid_token(email, password);

        // Act
        var res = await Cli
            .Request("account", "info")
            .WithOAuthBearerToken(token.AccessToken)
            .GetAsync();

        // Assert
        await Then_status_code_is(res, HttpStatusCode.OK);

        var info = await res.GetJsonAsync<AccountInfoResponse>();
        info.Should().NotBeNull();
        info.Email.Should().Be(email);
        info.Username.Should().Be(email);
        info.HasPassword.Should().BeTrue();
        info.Roles.Should().NotBeNull();
        info.Logins.Should().NotBeNull();
    }
}
