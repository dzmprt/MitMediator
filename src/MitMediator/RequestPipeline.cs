namespace MitMediator;

public struct RequestPipeline<TRequest, TResponse> : IRequestHandlerNext<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerator<IPipelineBehavior<TRequest, TResponse>> _behaviors;
    private readonly IRequestHandler<TRequest, TResponse> _handler;

    public RequestPipeline(
        IEnumerator<IPipelineBehavior<TRequest, TResponse>> behaviors,
        IRequestHandler<TRequest, TResponse> handler)
    {
        _behaviors = behaviors;
        _handler = handler;
    }

    public ValueTask<TResponse> InvokeAsync(TRequest request, CancellationToken cancellationToken)
    {
        if (_behaviors.MoveNext())
        {
            return _behaviors.Current.HandleAsync(request, this, cancellationToken);
        }

        return _handler.HandleAsync(request, cancellationToken);
    }
}