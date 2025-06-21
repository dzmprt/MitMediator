namespace MitMediator;

/// <summary>
/// Defines a mediator to encapsulate request/response and publishing interaction patterns.
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Asynchronously send a request to a single handler.
    /// </summary>
    /// <param name="request">Request object.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <typeparam name="TRequest"><see cref="TRequest"/></typeparam>
    /// <typeparam name="TResponse">Response type.</typeparam>
    /// <returns>A task that represents the send operation. The task result contains the handler response.</returns>
    Task<TResponse> Send<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken) where TRequest : IRequest<TResponse>;
    
    /// <summary>
    /// Asynchronously send a request to a single handler.
    /// </summary>
    /// <param name="request">Request object.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <typeparam name="TRequest"><see cref="TRequest"/></typeparam>
    /// <returns>A task that represents the send operation. The task result contains the handler response.</returns>
    Task Send<TRequest>(TRequest request, CancellationToken cancellationToken) where TRequest : IRequest;
    
    /// <summary>
    /// Asynchronously send a request to a single handler.
    /// </summary>
    /// <param name="request">Request object.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <typeparam name="TRequest"><see cref="TRequest"/></typeparam>
    /// <typeparam name="TResponse">Response type.</typeparam>
    /// <returns>A task that represents the send operation. The task result contains the handler response.</returns>
    ValueTask<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken) where TRequest : IRequest<TResponse>;
    
    /// <summary>
    /// Asynchronously send a request to a single handler.
    /// </summary>
    /// <param name="request">Request object.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <typeparam name="TRequest"><see cref="TRequest"/></typeparam>
    /// <returns>A task that represents the send operation. The task result contains the handler response.</returns>
    ValueTask SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken) where TRequest : IRequest;
    
    /// <summary>
    /// Asynchronously publishes a notification to all registered handlers.
    /// </summary>
    /// <param name="notification">Notification.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <typeparam name="TNotification">Notification type.</typeparam>
    /// <returns></returns>
    ValueTask PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification;

    /// <summary>
    /// Asynchronously publishes a notification to all registered handlers in parallel.
    /// </summary>
    /// <param name="notification">Notification.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <typeparam name="TNotification">Notification type.</typeparam>
    /// <returns></returns>
    Task PublishParallelAsync<TNotification>(
        TNotification notification,
        CancellationToken cancellationToken = default)
        where TNotification : INotification;

    /// <summary>
    /// Creates an asynchronous stream using a single stream request handler.
    /// </summary>
    /// <typeparam name="TResponse">Response type.</typeparam>
    /// <typeparam name="TRequest">Request type.</typeparam>
    /// <param name="request">Request type.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns>Returns an <see cref="IAsyncEnumerable{TResponse}"/> based on the specified stream request.</returns>
    IAsyncEnumerable<TResponse> CreateStream<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IStreamRequest<TResponse>;
}