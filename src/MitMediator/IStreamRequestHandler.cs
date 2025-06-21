namespace MitMediator;

/// <summary>
/// Handler for a request with a streaming response.
/// </summary>
/// <typeparam name="TRequest">Request type.</typeparam>
/// <typeparam name="TResponse">Response type.</typeparam>
public interface IStreamRequestHandler<in TRequest, out TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    /// <summary>
    /// Handles a stream request.
    /// </summary>
    /// <param name="request">Request</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns>Response from the request.</returns>
    IAsyncEnumerable<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}