using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Aufy.Core;

public static class IdentityResultExtensions
{
    public static ProblemDetails ToValidationProblem(this IdentityResult result)
    {
        var problem = new ValidationProblemDetails();
        if (result.Succeeded)
        {
            return problem;
        }
        
        foreach (var error in result.Errors)
        {
            if (problem.Errors.TryGetValue(error.Code, out var problemError))
            {
               problem.Errors[error.Code] = problemError.Append(error.Description).ToArray();
            }
            else
            {
                problem.Errors.Add(error.Code, new[] { error.Description });
            }
        }
        
        return problem;
    }
    
    public static ProblemDetails ToValidationProblem(this SignInResult result)
    {
        var problem = new ValidationProblemDetails();
        if (result.Succeeded)
        {
            return problem;
        }
        
        problem.Errors.Add("SignIn", new[] { "Invalid email or password" });
        
        return problem;
    }
}