using System.Net;
using Aufy.Core.AuthSchemes;
using Aufy.Core.Endpoints;
using FluentAssertions;
using Flurl.Http;

namespace TestWebApi.IntegrationTests;

public static class TestHelper
{
    public static async Task Given_user_signedUp_correctly(this FlurlClient cli, string email, string password)
    {
        var signupRes = await cli
            .Request("auth", "signup")
            .PostJsonAsync(new SignUpRequest
            {
                Email = email,
                Password = password
            });

        // Assert
        signupRes.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    public static async Task<IFlurlResponse> When_POST(this FlurlClient cli, string path, object body)
    {
        return await cli.Request(path).PostJsonAsync(body);
    }
    
    public static async Task<(string, FlurlCookie)> Given_user_signin_with_token(this FlurlClient cli, string email, string password)
    {
        var res = await cli
            .Request("auth", "signin")
            .PostJsonAsync(new SignInRequest
            {
                Email = email,
                Password = password
            });

        res.StatusCode.Should().Be((int)HttpStatusCode.OK);
        
        var token = await res.GetJsonAsync<AccessTokenResponse>();
        token.Should().NotBeNull();
        token.AccessToken.Should().NotBeNullOrEmpty();
        res.Cookies.Should().NotBeEmpty();
        res.Cookies.Should().Contain(c => c.Name == AufyAuthSchemeDefaults.RefreshTokenCookieName);
        
        var refreshToken = res.Cookies.FirstOrDefault(c => c.Name == AufyAuthSchemeDefaults.RefreshTokenCookieName);
        
        return (token.AccessToken, refreshToken!);
    }

    public static async Task<AccessTokenResponse> Given_user_gets_valid_token(this FlurlClient cli, string email, string password)
    {
        var res = await cli
            .Request("auth", "token")
            .PostJsonAsync(new TokenRequest
            {
                Email = email,
                Password = password,
            });

        res.StatusCode.Should().Be((int)HttpStatusCode.OK);
        var token = await res.GetJsonAsync<AccessTokenResponse>();
        
        token.Should().NotBeNull();
        token.AccessToken.Should().NotBeNullOrEmpty();
        token.RefreshToken.Should().NotBeNullOrEmpty();

        return token;
    }
}