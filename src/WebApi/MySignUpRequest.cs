using Aufy.Core.Endpoints;

namespace WebApi;

public interface IMySignUpRequest
{
    string? AboutMe { get; set; }
    string? MySiteUrl { get; set; }
}

public record MySignUpRequest : SignUpRequest, IMySignUpRequest
{
    public string? AboutMe { get; set; }
    public string? MySiteUrl { get; set; }
}

public record MySignUpExternalRequest : IMySignUpRequest
{
    public string? AboutMe { get; set; }
    public string? MySiteUrl { get; set; }
}