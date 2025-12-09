using System.Net;
using System.Text;
using FluentAssertions;
using Flurl;
using Flurl.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace TestWebApi.IntegrationTests.Tests;

internal class EmailConfirmEndpointTests : TestBase
{
    [Test]
    public async Task ValidRequest_ShouldConfirmEmail()
    {
        // Arrange
        var email = TestId.Format("test@test.test");
        var password = "SuperSecret(%2";
        await Cli.Given_user_signedUp_correctly(email, password);
        
        var confirmLink = TestAufyEmailSenderManager.GetEmailConfirmationLink(email);
        confirmLink.Should().NotBeNullOrWhiteSpace();
        
        // Act
        var confirmEmailRes = await Cli.Request(confirmLink).GetAsync();
        
        // Assert
        await Then_status_code_is(confirmEmailRes, HttpStatusCode.OK);
        
        // Verify email is now confirmed by trying to get a new confirmation link
        // (confirmed emails should not generate new confirmation emails)
        var resendRes = await Cli
            .Request("account", "email", "confirm", "resend")
            .PostJsonAsync(new { Email = email });
        
        resendRes.StatusCode.Should().Be((int)HttpStatusCode.OK);
        
        // Should still be only 1 confirmation email sent (the original one)
        TestAufyEmailSenderManager.GetSendEmailConfirmationAsyncCount(email).Should().Be(1);
    }
    
    [Test]
    public async Task InvalidUserId_ShouldReturnNotFound()
    {
        // Arrange
        var email = TestId.Format("test@test.test");
        var password = "SuperSecret(%2";
        await Cli.Given_user_signedUp_correctly(email, password);
        
        var confirmLink = TestAufyEmailSenderManager.GetEmailConfirmationLink(email);
        confirmLink.Should().NotBeNullOrWhiteSpace();
        
        var url = new Url(confirmLink);
        var code = url.QueryParams.FirstOrDefault("code");
        var invalidUserId = Guid.NewGuid().ToString(); // Non-existent user ID
        
        // Act
        var confirmEmailRes = await Cli
            .Request("account", "email", "confirm")
            .SetQueryParam("code", code)
            .SetQueryParam("userId", invalidUserId)
            .GetAsync();
        
        // Assert
        await Then_status_code_is(confirmEmailRes, HttpStatusCode.NotFound);
    }
    
    [Test]
    public async Task InvalidCode_ShouldReturnNotFound()
    {
        // Arrange
        var email = TestId.Format("test@test.test");
        var password = "SuperSecret(%2";
        await Cli.Given_user_signedUp_correctly(email, password);
        
        var confirmLink = TestAufyEmailSenderManager.GetEmailConfirmationLink(email);
        confirmLink.Should().NotBeNullOrWhiteSpace();
        
        var url = new Url(confirmLink);
        var userId = url.QueryParams.FirstOrDefault("userId");
        var invalidCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes("invalid-confirmation-code"));
        
        // Act
        var confirmEmailRes = await Cli
            .Request("account", "email", "confirm")
            .SetQueryParam("code", invalidCode)
            .SetQueryParam("userId", userId)
            .GetAsync();
        
        // Assert
        await Then_status_code_is(confirmEmailRes, HttpStatusCode.NotFound);
    }
    
    [Test]
    public async Task MissingCode_ShouldReturnNotFound()
    {
        // Arrange
        var email = TestId.Format("test@test.test");
        var password = "SuperSecret(%2";
        await Cli.Given_user_signedUp_correctly(email, password);
        
        var confirmLink = TestAufyEmailSenderManager.GetEmailConfirmationLink(email);
        confirmLink.Should().NotBeNullOrWhiteSpace();
        
        var url = new Url(confirmLink);
        var userId = url.QueryParams.FirstOrDefault("userId");
        
        // Act
        var confirmEmailRes = await Cli
            .Request("account", "email", "confirm")
            .SetQueryParam("userId", userId)
            // Missing code parameter
            .GetAsync();
        
        // Assert
        await Then_status_code_is(confirmEmailRes, HttpStatusCode.NotFound);
    }
    
    [Test]
    public async Task MissingUserId_ShouldReturnNotFound()
    {
        // Arrange
        var email = TestId.Format("test@test.test");
        var password = "SuperSecret(%2";
        await Cli.Given_user_signedUp_correctly(email, password);

        var confirmLink = TestAufyEmailSenderManager.GetEmailConfirmationLink(email);
        confirmLink.Should().NotBeNullOrWhiteSpace();
        
        var url = new Url(confirmLink);
        var code = url.QueryParams.FirstOrDefault("code");
        
        // Act
        var confirmEmailRes = await Cli
            .Request("account", "email", "confirm")
            .SetQueryParam("code", code)
            // Missing userId parameter
            .GetAsync();
        
        // Assert
        await Then_status_code_is(confirmEmailRes, HttpStatusCode.NotFound);
    }
    
    [Test]
    public async Task EmptyCode_ShouldReturnNotFound()
    {
        // Arrange
        var email = TestId.Format("test@test.test");
        var password = "SuperSecret(%2";
        await Cli.Given_user_signedUp_correctly(email, password);
        
        var confirmLink = TestAufyEmailSenderManager.GetEmailConfirmationLink(email);
        confirmLink.Should().NotBeNullOrWhiteSpace();
        
        var url = new Url(confirmLink);
        var userId = url.QueryParams.FirstOrDefault("userId");
        
        // Act
        var confirmEmailRes = await Cli
            .Request("account", "email", "confirm")
            .SetQueryParam("code", "")
            .SetQueryParam("userId", userId)
            .GetAsync();
        
        // Assert - Empty code results in user not found (404)
        await Then_status_code_is(confirmEmailRes, HttpStatusCode.NotFound);
    }
    
    [Test]
    public async Task EmptyUserId_ShouldReturnNotFound()
    {
        // Arrange
        var email = TestId.Format("test@test.test");
        var password = "SuperSecret(%2";
        await Cli.Given_user_signedUp_correctly(email, password);
        
        var confirmLink = TestAufyEmailSenderManager.GetEmailConfirmationLink(email);
        confirmLink.Should().NotBeNullOrWhiteSpace();
        
        var url = new Url(confirmLink);
        var code = url.QueryParams.FirstOrDefault("code");
        
        // Act
        var confirmEmailRes = await Cli
            .Request("account", "email", "confirm")
            .SetQueryParam("code", code)
            .SetQueryParam("userId", "")
            .GetAsync();
        
        // Assert - Empty userId results in user not found (404)
        await Then_status_code_is(confirmEmailRes, HttpStatusCode.NotFound);
    }
    
    [Test]
    public async Task AlreadyConfirmedEmail_ShouldReturnNotFound()
    {
        // Arrange
        var email = TestId.Format("test@test.test");
        var password = "SuperSecret(%2";
        await Cli.Given_user_signedUp_correctly(email, password);
        
        var confirmLink = TestAufyEmailSenderManager.GetEmailConfirmationLink(email);
        confirmLink.Should().NotBeNullOrWhiteSpace();
        
        // First confirmation should succeed
        var firstConfirmRes = await Cli.Request(confirmLink).GetAsync();
        firstConfirmRes.StatusCode.Should().Be((int)HttpStatusCode.OK);
        
        // Act - Try to confirm again with the same link
        var secondConfirmRes = await Cli.Request(confirmLink).GetAsync();
        
        // Assert
        await Then_status_code_is(secondConfirmRes, HttpStatusCode.NotFound);
    }
    
    [Test]
    public async Task MalformedCode_ShouldReturnInternalServerError()
    {
        // Arrange
        var email = TestId.Format("test@test.test");
        var password = "SuperSecret(%2";
        await Cli.Given_user_signedUp_correctly(email, password);
        
        var confirmLink = TestAufyEmailSenderManager.GetEmailConfirmationLink(email);
        confirmLink.Should().NotBeNullOrWhiteSpace();
        
        var url = new Url(confirmLink);
        var userId = url.QueryParams.FirstOrDefault("userId");
        var malformedCode = "not-base64-url-encoded-code";
        
        // Act
        var confirmEmailRes = await Cli
            .Request("account", "email", "confirm")
            .SetQueryParam("code", malformedCode)
            .SetQueryParam("userId", userId)
            .GetAsync();
        
        // Assert - Malformed base64 code causes exception (500)
        await Then_status_code_is(confirmEmailRes, HttpStatusCode.InternalServerError);
    }
    
    [Test]
    public async Task ValidCodeDifferentUser_ShouldReturnNotFound()
    {
        // Arrange
        var email1 = TestId.Format("test1@test.test");
        var email2 = TestId.Format("test2@test.test");
        var password = "SuperSecret(%2";
        
        await Cli.Given_user_signedUp_correctly(email1, password);
        await Cli.Given_user_signedUp_correctly(email2, password);
        
        var confirmLink1 = TestAufyEmailSenderManager.GetEmailConfirmationLink(email1);
        var confirmLink2 = TestAufyEmailSenderManager.GetEmailConfirmationLink(email2);
        
        confirmLink1.Should().NotBeNullOrWhiteSpace();
        confirmLink2.Should().NotBeNullOrWhiteSpace();
        
        var url1 = new Url(confirmLink1);
        var url2 = new Url(confirmLink2);
        
        var code1 = url1.QueryParams.FirstOrDefault("code");
        var userId2 = url2.QueryParams.FirstOrDefault("userId");
        
        // Act - Try to use user1's code with user2's userId
        var confirmEmailRes = await Cli
            .Request("account", "email", "confirm")
            .SetQueryParam("code", code1)
            .SetQueryParam("userId", userId2)
            .GetAsync();
        
        // Assert
        await Then_status_code_is(confirmEmailRes, HttpStatusCode.NotFound);
    }
}
