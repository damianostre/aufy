using Aufy.Core.Endpoints;

namespace Aufy.Server.Extended;

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

public class MySignUpExternalRequest : SignUpExternalRequest, IMySignUpRequest
{
    public string? AboutMe { get; set; }
    public string? MySiteUrl { get; set; }
}