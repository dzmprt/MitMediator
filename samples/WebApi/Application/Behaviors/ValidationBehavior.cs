using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MitMediator;

namespace Application.Behaviors;

/// <summary>
/// Request validation pipeline behavior. 
/// </summary>
/// <typeparam name="TRequest">Request type.</typeparam>
/// <typeparam name="TResponse">Response type.</typeparam>
internal class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationBehavior"/>.
    /// </summary>
    /// <param name="serviceProvider"><see cref="IServiceProvider"/>.</param>
    public ValidationBehavior(IServiceProvider serviceProvider)
    {
        var s = serviceProvider.GetServices<IValidator<TRequest>>();
        _validators = s;
    }

    /// <inheritdoc/>
    public async ValueTask<TResponse> HandleAsync(TRequest request, IRequestHandlerNext<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(result => result.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0) throw new ValidationException(failures);

        return await next.InvokeAsync(request, cancellationToken);    
    }
}