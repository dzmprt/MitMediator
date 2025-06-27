namespace MitMediator;

public interface IRequestHandlerNext<in TRequest, TResponse>
{
    ValueTask<TResponse> InvokeAsync(TRequest newRequest, CancellationToken cancellationToken);
}