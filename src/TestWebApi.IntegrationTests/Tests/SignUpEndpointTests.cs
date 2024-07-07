using System.Net;
using Aufy.Core.Endpoints;
using FluentAssertions;
using Flurl.Http;

namespace TestWebApi.IntegrationTests.Tests;

internal class SignUpEndpointTests : TestBase
{
    [Test]
    public async Task ValidRequest_ShouldAllowToSignIn()
    {
        // Arrange
        var email = TestId.Format("test@test.test");
        var password = "SuperSecret(%2";

        // Act
        var signupRes = await Cli
            .Request("auth", "signup")
            .PostJsonAsync(new SignUpRequest
            {
                Email = email,
                Password = password
            });

        // Assert
        await Then_status_code_is(signupRes, HttpStatusCode.OK);


        var tokenRes = await Cli
            .Request("auth", "signin")
            .PostJsonAsync(new SignInRequest
            {
                Email = email,
                Password = password
            });

        tokenRes.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }
}