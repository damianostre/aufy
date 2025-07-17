using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Aufy.Core.Endpoints;

/// <summary>
/// Extension point for the SignUpEndpoint.
/// </summary>
/// <typeparam name="TUser"></typeparam>
/// <typeparam name="TModel"></typeparam>
public interface ISignUpEvents<in TUser, in TModel> where TUser : IAufyUser
{
    /// <summary>
    /// Called when a user is being created. <br/>
    /// Return a ProblemHttpResult if the user can't be created.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="model"></param>
    /// <param name="httpRequest"></param>
    /// <returns></returns>
    Task<ProblemHttpResult?> UserCreatingAsync(TUser user, TModel model, HttpRequest httpRequest)
    {
        return Task.FromResult<ProblemHttpResult?>(null);
    }

    /// <summary>
    /// Called when a user is created and saved to the database.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="model"></param>
    /// <param name="httpRequest"></param>
    /// <returns></returns>
    Task UserCreatedAsync(TUser user, TModel model, HttpRequest httpRequest)
    {
        return Task.CompletedTask;
    }
}