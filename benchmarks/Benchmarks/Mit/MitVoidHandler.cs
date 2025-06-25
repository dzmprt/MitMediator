using MitMediator;

namespace Benchmarks.Mit;

public record MitVoidCommand(string Message) : IRequest;

public class MitVoidHandler : MitMediator.IRequestHandler<MitVoidCommand>
{
    public ValueTask HandleAsync(MitVoidCommand request, CancellationToken cancellationToken) =>
        ValueTask.CompletedTask;
}