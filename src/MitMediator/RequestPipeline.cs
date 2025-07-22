namespace MitMediator;

internal readonly struct RequestPipeline<TRequest, TResponse>(
    IEnumerator<IPipelineBehavior<TRequest, TResponse>> behaviors,
    IRequestHandler<TRequest, TResponse> handler)
    : IRequestHandlerNext<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public ValueTask<TResponse> InvokeAsync(TRequest request, CancellationToken cancellationToken)
    {
        if (behaviors.MoveNext())
        {
            return behaviors.Current.HandleAsync(request, this, cancellationToken);
        }

        return handler.HandleAsync(request, cancellationToken);
    }
}
