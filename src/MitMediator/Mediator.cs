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
        
        var requestHandler = _serviceProvider.GetRequiredService<IHandler<TRequest>>();

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

        if (!handlers.Any())
        {
            return Task.CompletedTask;
        }

        if (handlers.Length == 1)
        {
            return handlers.First().HandleAsync(notification, cancellationToken).AsTask();
        }
        
        var tasks = handlers.Select(h => h.HandleAsync(notification, cancellationToken).AsTask()).ToArray();
        return Task.WhenAll(tasks);
    }
}