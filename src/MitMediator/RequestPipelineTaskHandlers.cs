namespace MitMediator;

internal readonly struct RequestPipelineTaskHandlers<TRequest, TResponse> : IRequestHandlerNext<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerator<IPipelineBehavior<TRequest, TResponse>> _behaviors;
    private readonly Tasks.IRequestHandler<TRequest, TResponse> _handler;

    public RequestPipelineTaskHandlers(
        IEnumerator<IPipelineBehavior<TRequest, TResponse>> behaviors,
        Tasks.IRequestHandler<TRequest, TResponse> handler)
    {
        _behaviors = behaviors;
        _handler = handler;
    }

    public async ValueTask<TResponse> InvokeAsync(TRequest request, CancellationToken cancellationToken)
    {
        if (_behaviors.MoveNext())
        {
            return await _behaviors.Current.HandleAsync(request, this, cancellationToken);
        }

        return await _handler.Handle(request, cancellationToken);
    }
}
