using System.Runtime.CompilerServices;
using MitMediator;

namespace Benchmarks.Mit;

public record MitMediatorStreamRequest(int Count) : IStreamRequest<int>;

public class MitMediatorStreamBehavior : IStreamPipelineBehavior<MitMediatorStreamRequest, int>
{
    public async IAsyncEnumerable<int> HandleAsync(
        MitMediatorStreamRequest request,
        IAsyncEnumerable<int> nextPipe,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var item in nextPipe.WithCancellation(cancellationToken))
        {
            yield return item * 2;
        }
    }
}

public class MitMediatorStreamQueryHandler : IStreamRequestHandler<MitMediatorStreamRequest, int>
{
    public async IAsyncEnumerable<int> HandleAsync(MitMediatorStreamRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        for (var i = 0; i < request.Count; i++)
        {
            yield return i * 3;
        }
    }
}