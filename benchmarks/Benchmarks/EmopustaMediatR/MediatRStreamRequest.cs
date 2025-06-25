using System.Runtime.CompilerServices;
using MediatR;

namespace Benchmarks.EmopustaMediatR;

public record MediatRStreamRequest(int Count) : IStreamRequest<int>;

public class MediatRStreamBehavior : IStreamPipelineBehavior<MediatRStreamRequest, int>
{
    public async IAsyncEnumerable<int> Handle(MediatRStreamRequest request, StreamHandlerDelegate<int> next, CancellationToken cancellationToken)
    {
        await foreach (var item in next.Invoke().WithCancellation(cancellationToken))
        {
            yield return item * 2;
        }
    }
}

public class MediatRStreamQueryHandler : IStreamRequestHandler<MediatRStreamRequest, int>
{
    public async IAsyncEnumerable<int> Handle(MediatRStreamRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        for (var i = 0; i < request.Count; i++)
        {
            yield return i * 3;
        }
    }
}