using Microsoft.Extensions.DependencyInjection;
using MitMediator.Tasks;

namespace MitMediator;

internal class Mediator(IServiceProvider serviceProvider) : IMediator
{
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
    {
        var requestType = request.GetType();

        var genericMethod = MediatRSendMethodInfo.SendMethod.GetOrAdd(requestType, _ =>
        {
            var responseType = typeof(TResponse);
            return MediatRSendMethodInfo.SendGenericMethod.MakeGenericMethod(requestType, responseType);
        });

        var result = genericMethod.Invoke(this, [request, cancellationToken])!;

        return (Task<TResponse>)result;
    }

    public Task<TResponse> Send<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        var behaviors = serviceProvider
            .GetServices<IPipelineBehavior<TRequest, TResponse>>();

        var requestHandler = serviceProvider
            .GetRequiredService<Tasks.IRequestHandler<TRequest, TResponse>>();

        using var behaviorEnumerator = behaviors.GetEnumerator();
        var pipeline = new RequestPipelineTaskHandlers<TRequest, TResponse>(behaviorEnumerator, requestHandler);
        return pipeline.InvokeAsync(request, cancellationToken).AsTask();
    }

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken) where TRequest : IRequest
    {
        var requestHandler = serviceProvider
            .GetRequiredService<Tasks.IRequestHandler<TRequest>>();

        var behaviors = serviceProvider
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
        var behaviors = serviceProvider
            .GetServices<IPipelineBehavior<TRequest, TResponse>>();

        var requestHandler = serviceProvider
            .GetService<IRequestHandler<TRequest, TResponse>>();

        using var behaviorEnumerator = behaviors.GetEnumerator();
        if (requestHandler is not null)
        {
            var pipeline = new RequestPipeline<TRequest, TResponse>(behaviorEnumerator, requestHandler);
            return pipeline.InvokeAsync(request, cancellationToken);
        }

        var requestHandlerTaskResult = serviceProvider
            .GetService<Tasks.IRequestHandler<TRequest, TResponse>>();

        if (requestHandlerTaskResult is not null)
        {
            var pipeline =
                new RequestPipelineTasks<TRequest, TResponse>(behaviorEnumerator, requestHandlerTaskResult);
            return pipeline.InvokeAsync(request, cancellationToken);
        }

        throw new InvalidOperationException(
            $"No handler for request {request.GetType().FullName} has been registered.");
    }

    public ValueTask<Unit> SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken)
        where TRequest : IRequest
    {
        return SendAsync<TRequest, Unit>(request, cancellationToken);
    }

    public async ValueTask PublishAsync<TNotification>(
        TNotification notification,
        CancellationToken cancellationToken)
        where TNotification : INotification
    {
        var notificationHandlers = serviceProvider.GetServices<INotificationHandler<TNotification>>();
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
        var handlers = serviceProvider
            .GetServices<INotificationHandler<TNotification>>()
            .ToArray();

        switch (handlers.Length)
        {
            case 0:
                return Task.CompletedTask;
            case 1:
                return handlers[0].HandleAsync(notification, cancellationToken).AsTask();
            default:
                var tasks = handlers.Select(h => h.HandleAsync(notification, cancellationToken).AsTask());
                return Task.WhenAll(tasks);
        }
    }

    public IAsyncEnumerable<TResponse> CreateStream<TRequest, TResponse>(TRequest request,
        CancellationToken cancellationToken) where TRequest : IStreamRequest<TResponse>
    {
        var behaviors = serviceProvider
            .GetServices<IStreamPipelineBehavior<TRequest, TResponse>>();

        var requestHandler = serviceProvider
            .GetRequiredService<IStreamRequestHandler<TRequest, TResponse>>();

        var next = requestHandler.HandleAsync(request, cancellationToken);

        foreach (var behavior in behaviors)
        {
            next = behavior.HandleAsync(request, next, cancellationToken);
        }

        return next;
    }
}
