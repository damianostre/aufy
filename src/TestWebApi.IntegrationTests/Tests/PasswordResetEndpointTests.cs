using System.Net;
using Aufy.Core.Endpoints;
using FluentAssertions;
using Flurl;
using Flurl.Http;

namespace TestWebApi.IntegrationTests.Tests;

internal class PasswordResetEndpointTests : TestBase
{
    [Test]
    public async Task WrongCode()
    {
        // Arrange
        var email = TestId.Format("test@test.test");
        
        //Act
        var res = await Cli
            .Request("account", "password", "reset")
            .PostJsonAsync(new PasswordResetRequest
            {
                Email = email,
                Password = "password",
                Code = "code"
            });
        
        // Assert
        res.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }
    
    
    [Test]
    public async Task AccountExists()
    {
        var email = TestId.Format("test@test.test");
        var password = "SuperSecret(%2";
        
        await Cli.Given_user_signedUp_correctly(email, password + password);
        
        //Call forgot password endpoint to get code
        var forgotPasswordRes = await Cli.When_POST("account/password/forgot", new PasswordForgotRequest
        {
            Email = email
        });
        await Then_status_code_is(forgotPasswordRes, HttpStatusCode.OK);

        var resetLink = TestAufyEmailSenderManager.GetResetPasswordLink(email);
        
        resetLink.Should().NotBeNullOrWhiteSpace();
        Url.IsValid(resetLink).Should().BeTrue();
        
        var url = new Url(resetLink);
        
        var codeParam = url.QueryParams.FirstOrDefault(p => p.Name == "code");
        codeParam.Should().NotBeNull();
        codeParam.Value.Should().NotBeNull();
        
        var res = await Cli.When_POST("account/password/reset", new PasswordResetRequest
        {
            Email = email,
            Password = password,
            Code = codeParam.Value.ToString()
        });

        await Then_status_code_is(res, HttpStatusCode.OK);

        
        await Cli.Given_user_gets_valid_token(email, password);
    }
}   