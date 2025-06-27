using MitMediator;

namespace Benchmarks.Mit;

public class MitPipelineBehaviorFirst<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public async ValueTask<TResponse> HandleAsync(TRequest request, IRequestHandlerNext<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
        return await next.InvokeAsync(request, cancellationToken);
    }
}

public class MitPipelineBehaviorSecond<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public async ValueTask<TResponse> HandleAsync(TRequest request, IRequestHandlerNext<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
        return await next.InvokeAsync(request, cancellationToken);
    }
}