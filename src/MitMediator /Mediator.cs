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
            .Distinct()
            .ToArray();

        var handler = async ValueTask<TResponse> () =>
        {
            var requestHandler = _serviceProvider
                .GetRequiredService<Tasks.IRequestHandler<TRequest, TResponse>>();
            return await requestHandler.Handle(request, cancellationToken);
        };

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
            .Distinct()
            .ToArray();

        Func<ValueTask<Unit>> handler = async () =>
        {
            var requestHandler = _serviceProvider
                .GetRequiredService<Tasks.IRequestHandler<TRequest>>();
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
            .Distinct()
            .ToArray();

        var handler = () =>
        {
            var requestHandler = _serviceProvider.GetRequiredService<IHandler<TRequest, TResponse>>();
            return requestHandler.HandleAsync(request, cancellationToken);
        };

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
            .Distinct()
            .ToArray();

        Func<ValueTask<Unit>> handler = async () =>
        {
            var requestHandler = _serviceProvider.GetRequiredService<IHandler<TRequest>>();
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
}