namespace MitMediator;

public delegate ValueTask<TResponse> RequestHandlerDelegate<in TRequest, TResponse>(
    TRequest request,
    CancellationToken cancellationToken
) where TRequest : IRequest<TResponse>;