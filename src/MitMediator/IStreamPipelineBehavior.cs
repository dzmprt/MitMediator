namespace MitMediator;

/// <summary>
/// Pipeline behavior to surround the inner handler. 
/// </summary>
/// <typeparam name="TRequest">Request type.</typeparam>
/// <typeparam name="TResponse">Response type.</typeparam>
public interface IStreamPipelineBehavior<in TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    /// <summary>
    /// Pipeline handler. Perform any additional behavior.
    /// </summary>
    /// <param name="request">Incoming request.</param>
    /// <param name="next">Delegate for the next action in the pipeline. Eventually this delegate represents the handler.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns>Awaitable task returning the TResponse</returns>
    IAsyncEnumerable<TResponse> HandleAsync(TRequest request, IAsyncEnumerable<TResponse> next,
        CancellationToken cancellationToken);
}