using System.Net;
using FluentAssertions;
using Flurl;
using Flurl.Http;

namespace TestWebApi.IntegrationTests.Tests;

internal class ExternalChallengeEndpointTests : TestBase
{
    [Test]
    public async Task CallExternalChallenge_UnknownProvider_ShouldReturn404()
    {
        // Act
        var res = await Cli
            .Request("auth", "external", "challenge", "unknown")
            .SetQueryParam("callbackUrl", "https://example.com/callback")
            .GetAsync();

        // Assert
        await Then_status_code_is(res, HttpStatusCode.NotFound);
    }

    [Test]
    public async Task CallExternalChallenge_KnownProvider_ShouldReturn302_AndRedirectToCallback()
    {
        var callback = "http://localhost/api/auth/external/callback";

        var res = await Cli
            .WithAutoRedirect(false)
            .Request("auth", "external", "challenge", "github")
            .SetQueryParam("callbackUrl", "https://example.com/callback")
            .GetAsync();

        await Then_status_code_is(res, HttpStatusCode.Redirect);

        // Location header should match callback
        var location = res.ResponseMessage.Headers.Location?.ToString();
        location.Should().Contain(Url.Encode(callback));
        location.Should().Contain("github.com");
    }
}
