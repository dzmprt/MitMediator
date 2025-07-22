namespace MitMediator;

internal readonly struct RequestPipelineTaskHandlers<TRequest, TResponse>(
    IEnumerator<IPipelineBehavior<TRequest, TResponse>> behaviors,
    Tasks.IRequestHandler<TRequest, TResponse> handler) : IRequestHandlerNext<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async ValueTask<TResponse> InvokeAsync(TRequest request, CancellationToken cancellationToken)
    {
        if (behaviors.MoveNext())
        {
            return await behaviors.Current.HandleAsync(request, this, cancellationToken);
        }

        return await handler.Handle(request, cancellationToken);
    }
}
