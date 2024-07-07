using System.Net;
using Aufy.Server.IntegrationTests;
using FluentAssertions;
using Flurl.Http;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TestWebApi.IntegrationTests;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
internal class TestBase : IDisposable
{
    protected MyWebApplicationFactory App { get; set; } = MyWebApplicationFactory.Instance.Value;
    protected FlurlClient Cli { get; set; }
    protected TestId TestId { get; set; }
    public IFlurlResponse? HttpResponse { get; set; }

    public TestBase()
    {
        Cli = new FlurlClient(
                App.CreateClient(new WebApplicationFactoryClientOptions {AllowAutoRedirect = false}),
                App.Server.BaseAddress + "api")
            .AllowAnyHttpStatus();
        TestId = new TestId();
    }

    public Task Then_status_code_is(IFlurlResponse response, HttpStatusCode status)
    {
        HttpResponse = response;
        response.StatusCode.Should().Be((int) status);

        return Task.CompletedTask;
    }

    [TearDown]
    public async Task TearDown()
    {
        if (TestContext.CurrentContext.Result.FailCount > 0
            && HttpResponse is not null)
        {
            await TestContext.Out.WriteLineAsync("Response message: ");
            await TestContext.Out.WriteLineAsync(HttpResponse.ResponseMessage.ToString());
            await TestContext.Out.WriteLineAsync("Response content: ");
            await TestContext.Out.WriteLineAsync(await HttpResponse.ResponseMessage.Content.ReadAsStringAsync());
        }
    }

    public void Dispose()
    {
        Cli.Dispose();
    }
}