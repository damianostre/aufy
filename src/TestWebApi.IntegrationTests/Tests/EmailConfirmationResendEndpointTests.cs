using System.Net;
using Aufy.Core.Endpoints;
using FluentAssertions;
using Flurl;
using Flurl.Http;

namespace TestWebApi.IntegrationTests.Tests;

internal class EmailConfirmationResendEndpointTests : TestBase
{
    [Test]
    public async Task EmailNotConfirmed_ShouldSendEmail()
    {
        var email = TestId.Format("test@test.test");
        var password = "SuperSecret(%2";
        
        await Cli.Given_user_signedUp_correctly(email, password);
        
        //resend email
        var tokenRes = await Cli
            .Request("account", "email", "confirm", "resend")
            .PostJsonAsync(new EmailConfirmationResendRequest
            {
                Email = email
            });

        await Then_status_code_is(tokenRes, HttpStatusCode.OK);
        
        //2 Because we already sent email confirmation on signup
        TestAufyEmailSenderManager.GetSendEmailConfirmationAsyncCount(email).Should().Be(2);
    }
    
    //Email confirmed
    [Test]
    public async Task EmailConfirmed_ShouldNotSendEmail()
    {
        var email = TestId.Format("test@test.test");
        var password = "SuperSecret(%2";
        
        await Cli.Given_user_signedUp_correctly(email, password);
        
        var confirmLink = TestAufyEmailSenderManager.GetEmailConfirmationLink(email);
        confirmLink.Should().NotBeNullOrWhiteSpace();
        Url.IsValid(confirmLink).Should().BeTrue();
        
        //Confirm email
        var confirmEmailRes = await Cli.HttpClient.GetAsync(confirmLink);
        
        confirmEmailRes.StatusCode.Should().Be(HttpStatusCode.OK);
        
        //resend email
        var tokenRes = await Cli
            .Request("account", "email", "confirm", "resend")
            .PostJsonAsync(new EmailConfirmationResendRequest
            {
                Email = email
            });
        
        await Then_status_code_is(tokenRes, HttpStatusCode.OK);
        
        //Only 1, the second call should not send email
        TestAufyEmailSenderManager.GetSendEmailConfirmationAsyncCount(email).Should().Be(1);
    }
}