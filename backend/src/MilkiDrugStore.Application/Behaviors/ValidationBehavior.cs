using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MilkiDrugStore.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that runs FluentValidation validators for every
/// command/query before the handler executes. Validation failures are surfaced
/// as a single <see cref="ValidationException"/> with a combined error message.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var failures = new List<ValidationFailure>();

            foreach (var validator in _validators)
            {
                try
                {
                    var result = await validator.ValidateAsync(context, cancellationToken);
                    foreach (var error in result.Errors)
                        failures.Add(error);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Validator {ValidatorType} threw an exception.", validator.GetType().Name);
                }
            }

            if (failures.Count > 0)
            {
                var message = string.Join("; ", failures
                    .GroupBy(f => f.PropertyName)
                    .Select(g => $"{string.Join(", ", g.Select(f => f.PropertyName))}: {string.Join(", ", g.Select(f => f.ErrorMessage))}"));

                throw new ValidationException(message);
            }
        }

        return await next();
    }
}