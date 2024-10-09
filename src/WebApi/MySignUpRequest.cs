using Aufy.Core.Endpoints;

namespace WebApi;

public interface IMySignUpRequest
{
    string? AboutMe { get; set; }
    string? MySiteUrl { get; set; }
}

public class MySignUpRequest : SignUpRequest, IMySignUpRequest
{
    public string? AboutMe { get; set; }
    public string? MySiteUrl { get; set; }
}

public class MySignUpExternalRequest : IMySignUpRequest
{
    public string? AboutMe { get; set; }
    public string? MySiteUrl { get; set; }
}