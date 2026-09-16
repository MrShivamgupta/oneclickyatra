using System.Collections;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;
using OneClickYatra.Api.Globals;

namespace OneClickYatra.Api.Infrastructure;

/// <summary>
/// Global action filter: for every action argument (or, for a list-bodied argument, every
/// element in it) that has a registered FluentValidation IValidator&lt;T&gt;, runs it before the
/// action executes. Centralizes server-side validation so controllers don't each have to call
/// validators manually.
/// </summary>
public sealed class ValidationActionFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationActionFilter(IServiceProvider __serviceProvider)
    {
        _serviceProvider = __serviceProvider;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext __context, ActionExecutionDelegate __next)
    {
        var errors = new Dictionary<string, string[]>();

        foreach (var argument in __context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            if (argument is IEnumerable enumerable and not string)
            {
                var index = 0;
                foreach (var element in enumerable)
                {
                    if (element is not null)
                    {
                        await ValidateAsync(element, errors, __prefix: $"[{index}].", __context.HttpContext.RequestAborted);
                    }
                    index++;
                }
                continue;
            }

            await ValidateAsync(argument, errors, __prefix: string.Empty, __context.HttpContext.RequestAborted);
        }

        if (errors.Count > 0)
        {
            throw new ValidationAppException(errors);
        }

        await __next();
    }

    private async Task ValidateAsync(object __argument, Dictionary<string, string[]> __errors, string __prefix, CancellationToken __cancellationToken)
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(__argument.GetType());
        if (_serviceProvider.GetService(validatorType) is not IValidator validator)
        {
            return;
        }

        var validationContext = new ValidationContext<object>(__argument);
        var result = await validator.ValidateAsync(validationContext, __cancellationToken);

        foreach (var group in result.Errors.GroupBy(failure => __prefix + failure.PropertyName))
        {
            __errors[group.Key] = group.Select(failure => failure.ErrorMessage).ToArray();
        }
    }
}
