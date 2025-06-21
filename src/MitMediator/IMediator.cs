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
}