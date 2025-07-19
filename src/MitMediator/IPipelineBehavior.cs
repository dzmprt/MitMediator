namespace MitMediator;

/// <summary>
/// Pipeline behavior to surround the inner handler. Implementations add additional behavior and await the next delegate.
/// </summary>
/// <typeparam name="TRequest">Request type.</typeparam>
/// <typeparam name="TResponse">Response type.</typeparam>
public interface IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Pipeline handler. Perform any additional behavior and await the next delegate as necessary.
    /// </summary>
    /// <param name="request">Incoming request.</param>
    /// <param name="nextPipe">Awaitable delegate for the next action in the pipeline. Eventually this delegate represents the handler.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
    /// <returns>Awaitable task returning the TResponse.</returns>
    ValueTask<TResponse> HandleAsync(TRequest request, IRequestHandlerNext<TRequest, TResponse> nextPipe, CancellationToken cancellationToken);
}
