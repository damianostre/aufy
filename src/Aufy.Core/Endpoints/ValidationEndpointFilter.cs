using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Aufy.Core.Endpoints;

public class ValidationEndpointFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var model = context.GetArgument<T>(0);
        var validationResults = new List<ValidationResult>();
        if (Validator.TryValidateObject(model, new ValidationContext(model), validationResults, true) || validationResults.Count == 0)
        {
            return await next(context);
        }

        var errors = validationResults.SelectMany(s => s.MemberNames.Select(m => new { MemberName = m, s.ErrorMessage }))
            .GroupBy(e => e.MemberName)
            .ToDictionary(
                g => g.Key, g
                    => g.Where(e => !string.IsNullOrWhiteSpace(e.ErrorMessage))
                        .Select(e => e.ErrorMessage!).ToArray());

        return TypedResults.ValidationProblem(
            errors: errors,
            title: "Error validating payload"
        );
    }
}