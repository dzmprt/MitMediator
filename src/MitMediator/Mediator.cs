using System.Buffers;
using System.Runtime.CompilerServices;
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
            .GetServices<IPipelineBehavior<TRequest, TResponse>>();
        
        var requestHandler = _serviceProvider
            .GetRequiredService<Tasks.IRequestHandler<TRequest, TResponse>>();
        
        var next = TaskToValueTask(requestHandler.Handle(request, cancellationToken));

        foreach (var behavior in behaviors)
        {
            next = behavior.HandleAsync(request, next, cancellationToken);
        }

        return ValueTaskToTask(next);
    }

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken) where TRequest : IRequest
    {
        var requestHandler = _serviceProvider
            .GetRequiredService<Tasks.IRequestHandler<TRequest>>();
        
        var behaviors = _serviceProvider
            .GetServices<IPipelineBehavior<TRequest, Unit>>();
        
        var next = TaskToValueTaskUnit(requestHandler.Handle(request, cancellationToken));

        foreach (var behavior in behaviors)
        {
            next = behavior.HandleAsync(request, next, cancellationToken);
        }

        return ValueTaskUnitToTask(next);
    }

    public ValueTask<TResponse> SendAsync<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        var requestHandler = _serviceProvider
            .GetRequiredService<IRequestHandler<TRequest, TResponse>>();
        
        var behaviors = _serviceProvider
            .GetServices<IPipelineBehavior<TRequest, TResponse>>();

        var next = requestHandler.HandleAsync(request, cancellationToken);

        foreach (var behavior in behaviors)
        {
            next = behavior.HandleAsync(request, next, cancellationToken);
        }

        return next;
    }
    
    public ValueTask SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken)
        where TRequest : IRequest
    {
        var requestHandler = _serviceProvider
            .GetRequiredService<IRequestHandler<TRequest>>();
        
        var behaviors = _serviceProvider
            .GetServices<IPipelineBehavior<TRequest, Unit>>();

        var next = ValueTaskToValueTaskUnit(requestHandler.HandleAsync(request, cancellationToken));

        foreach (var behavior in behaviors)
        {
            next = behavior.HandleAsync(request, next, cancellationToken);
        }

        return ValueTaskUnitToValueTask(next);
    }
    
    public ValueTask PublishAsync<TNotification>(
        TNotification notification, 
        CancellationToken cancellationToken)
        where TNotification : INotification
    {
        var notificationHandlers = _serviceProvider.GetServices<INotificationHandler<TNotification>>();
        
        return CombineNotificationsValueTask(notificationHandlers, notification, cancellationToken);
    }
    
    public Task PublishParallelAsync<TNotification>(
        TNotification notification,
        CancellationToken cancellationToken)
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

    public IAsyncEnumerable<TResponse> CreateStream<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken) where TRequest : IStreamRequest<TResponse>
    {
        var behaviors = _serviceProvider
            .GetServices<IStreamPipelineBehavior<TRequest, TResponse>>();
        
        var requestHandler = _serviceProvider
            .GetRequiredService<IStreamRequestHandler<TRequest, TResponse>>();

        var next = requestHandler.HandleAsync(request, cancellationToken);
        
        foreach (var behavior in behaviors)
        {
            next = behavior.HandleAsync(request, next, cancellationToken);
        }

        return next;
    }
    
    private static async ValueTask CombineNotificationsValueTask<TNotification>(IEnumerable<INotificationHandler<TNotification>> handlers, TNotification notification, CancellationToken ct) where TNotification : INotification
    {
        foreach (var handler in handlers)
        {
            await handler.HandleAsync(notification, ct);
        }
    }
    
    
    private static async ValueTask<Unit> ValueTaskToValueTaskUnit(ValueTask valueTask)
    {
        await valueTask;
        return new Unit();
    }
    
    private static async ValueTask<Unit> TaskToValueTaskUnit(Task task)
    {
        await task;
        return new Unit();
    }
    
    private static async ValueTask<TResponse> TaskToValueTask<TResponse>(Task<TResponse> valueTaskUnit)
    {
        return await valueTaskUnit;
    }
        
    private static async ValueTask ValueTaskUnitToValueTask(ValueTask<Unit> valueTaskUnit)
    {
        await valueTaskUnit;
    }
    
    private static async Task<TResponse> ValueTaskToTask<TResponse>(ValueTask<TResponse> valueTaskUnit)
    {
        return await valueTaskUnit;
    }
    
    private static async Task ValueTaskUnitToTask(ValueTask<Unit> valueTaskUnit)
    {
        await valueTaskUnit;
    }
}