using Microsoft.Extensions.DependencyInjection;

namespace MitMediator;

internal class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<TResponse> Send<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        var behaviors = _serviceProvider
            .GetServices<IPipelineBehavior<TRequest, TResponse>>()
            .Distinct();
        
        var requestHandler = _serviceProvider
            .GetRequiredService<Tasks.IRequestHandler<TRequest, TResponse>>();

        var handler = async ValueTask<TResponse> () => 
            await requestHandler.Handle(request, cancellationToken);

        foreach (var behavior in behaviors)
        {
            var next = handler;
            handler = () => behavior.HandleAsync(request, next, cancellationToken);
        }

        return handler().AsTask();
    }

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken) where TRequest : IRequest
    {
        var behaviors = _serviceProvider
            .GetServices<IPipelineBehavior<TRequest, Unit>>()
            .Distinct();
        
        var requestHandler = _serviceProvider
            .GetRequiredService<Tasks.IRequestHandler<TRequest>>();

        var handler = async ValueTask<Unit> () => 
        {
            await requestHandler.Handle(request, cancellationToken);
            return new Unit();
        };

        foreach (var behavior in behaviors)
        {
            var next = handler;
            handler = () => behavior.HandleAsync(request, next, cancellationToken);
        }

        return handler().AsTask();
    }

    public ValueTask<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        var behaviors = _serviceProvider
            .GetServices<IPipelineBehavior<TRequest, TResponse>>()
            .Distinct();
        
        var requestHandler = _serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();

        var handler = () => requestHandler.HandleAsync(request, cancellationToken);

        foreach (var behavior in behaviors)
        {
            var next = handler;
            handler = () => behavior.HandleAsync(request, next, cancellationToken);
        }

        return handler();
    }

    public async ValueTask SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken)
        where TRequest : IRequest
    {
        var behaviors = _serviceProvider
            .GetServices<IPipelineBehavior<TRequest, Unit>>()
            .Distinct();
        
        var requestHandler = _serviceProvider.GetRequiredService<IRequestHandler<TRequest>>();

        Func<ValueTask<Unit>> handler = async () =>
        {
            await requestHandler.HandleAsync(request, cancellationToken);
            return new Unit();
        };

        foreach (var behavior in behaviors)
        {
            var next = handler;
            handler = () => behavior.HandleAsync(request, next, cancellationToken);
        }

        await handler();
    }
    
    public async ValueTask PublishAsync<TNotification>(
        TNotification notification, 
        CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        var handlers = _serviceProvider
            .GetServices<INotificationHandler<TNotification>>();
        
        foreach (var handler in handlers)
        {
            await handler.HandleAsync(notification, cancellationToken);
        }
    }
    
    public Task PublishParallelAsync<TNotification>(
        TNotification notification,
        CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        var handlers = _serviceProvider
            .GetServices<INotificationHandler<TNotification>>()
            .ToArray();

        switch (handlers.Length)
        {
            case 0:
                return Task.CompletedTask;
            case 1:
                return handlers[0].HandleAsync(notification, cancellationToken).AsTask();
            default:
            {
                var tasks = handlers.Select(h => h.HandleAsync(notification, cancellationToken).AsTask());
                return Task.WhenAll(tasks);
            }
        }
    }

    public IAsyncEnumerable<TResponse> CreateStream<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IStreamRequest<TResponse>
    {
        var handler = _serviceProvider
            .GetRequiredService<IStreamRequestHandler<TRequest, TResponse>>();

        return handler.Handle(request, cancellationToken);
    }
}