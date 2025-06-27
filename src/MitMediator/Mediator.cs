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
        
        using var behaviorEnumerator = behaviors.GetEnumerator();
        var pipeline = new RequestPipelineTaskHandlers<TRequest, TResponse>(behaviorEnumerator, requestHandler);
        return pipeline.InvokeAsync(request, cancellationToken).AsTask();
    }

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken) where TRequest : IRequest
    {
        var requestHandler = _serviceProvider
            .GetRequiredService<Tasks.IRequestHandler<TRequest>>();
        
        var behaviors = _serviceProvider
            .GetServices<IPipelineBehavior<TRequest, Unit>>();
        
        using var behaviorEnumerator = behaviors.GetEnumerator();
        var pipeline = new RequestPipelineTaskHandlers<TRequest, Unit>(behaviorEnumerator, requestHandler);
        return pipeline.InvokeAsync(request, cancellationToken).AsTask();
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
        
        using var behaviorEnumerator = behaviors.GetEnumerator();
        var pipeline = new RequestPipeline<TRequest, TResponse>(behaviorEnumerator, requestHandler);
        return pipeline.InvokeAsync(request, cancellationToken);
    }
    
    public ValueTask<Unit> SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken)
        where TRequest : IRequest
    {
        var requestHandler = _serviceProvider
            .GetRequiredService<IRequestHandler<TRequest>>();
        
        var behaviors = _serviceProvider
            .GetServices<IPipelineBehavior<TRequest, Unit>>();

        using var behaviorEnumerator = behaviors.GetEnumerator();
        var pipeline = new RequestPipeline<TRequest, Unit>(behaviorEnumerator, requestHandler);
        return pipeline.InvokeAsync(request, cancellationToken);
    }
    
    public async ValueTask PublishAsync<TNotification>(
        TNotification notification, 
        CancellationToken cancellationToken)
        where TNotification : INotification
    {
        var notificationHandlers = _serviceProvider.GetServices<INotificationHandler<TNotification>>();
        foreach (var notificationHandler in notificationHandlers)
        {
            await notificationHandler.HandleAsync(notification, cancellationToken);
        }
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
}