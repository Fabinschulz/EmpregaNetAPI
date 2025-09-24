using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Infra.Components.Interfaces;
using FluentValidation;

namespace EmpregaNet.Application.Common.Behaviors
{
    /// <summary>
    /// Behavior for validating requests.
    /// </summary>
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (_validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);
                var validationResults = await Task.WhenAll(
                    _validators.Select(validator => validator.ValidateAsync(context, cancellationToken))
                );

                var failures = validationResults
                    .Where(result => result.Errors.Any())
                    .SelectMany(result => result.Errors)
                    .ToList();

                if (failures.Any())
                {
                    throw new ValidationAppException(failures);
                }
            }


            return await next();
        }

    }
}