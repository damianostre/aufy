using System.Net;
using Aufy.Core.Endpoints;
using FluentAssertions;
using Flurl.Http;

namespace TestWebApi.IntegrationTests.Tests;

internal class PasswordForgotEndpointTests : TestBase
{
    [Test]
    public async Task AccountExists()
    {
        // Arrange
        var email = TestId.Format("test@test.test");
        var password = "SuperSecret(%2";
        var signupRes = await Cli
            .Request("auth", "signup")
            .PostJsonAsync(new SignUpRequest
            {
                Email = email,
                Password = password,
            });
        await Then_status_code_is(signupRes, HttpStatusCode.OK);


        var tokenRes = await Cli
            .Request("account", "password", "forgot")
            .PostJsonAsync(new PasswordForgotRequest
            {
                Email = email
            });

        tokenRes.StatusCode.Should().Be((int)HttpStatusCode.OK);
        TestAufyEmailSenderManager.GetSendPasswordForgotAsyncCount(email).Should().Be(1);
    }
}