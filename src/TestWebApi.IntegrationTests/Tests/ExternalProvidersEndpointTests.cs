using System.Net;
using Aufy.Core.Endpoints;
using FluentAssertions;
using Flurl.Http;

namespace TestWebApi.IntegrationTests.Tests;

internal class ExternalProvidersEndpointTests : TestBase
{
    [Test]
    public async Task GetProviders_ShouldReturn200_AndContainConfiguredProviders()
    {
        // Act
        var res = await Cli
            .Request("auth", "external", "providers")
            .GetAsync();

        // Assert status
        await Then_status_code_is(res, HttpStatusCode.OK);

        // Assert payload
        var payload = await res.GetJsonAsync<ExternalProvidersResponse>();
        payload.Should().NotBeNull();
        payload.Providers.Should().NotBeNull();
        payload.Providers.Should().NotBeEmpty();
        // Program.cs registers GitHub and Discord providers for tests
        payload.Providers.Select(p => p.ToLower()).Should().Contain(new[] { "github", "discord" });
    }

    [Test]
    public async Task GetProviders_Anonymous_ShouldSucceed()
    {
        // No auth headers/cookies set in client => anonymous request
        var res = await Cli
            .Request("auth", "external", "providers")
            .GetAsync();

        await Then_status_code_is(res, HttpStatusCode.OK);
    }
}
