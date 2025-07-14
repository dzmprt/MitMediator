using MitMediator;

namespace Benchmarks.Mit;

public record MitVoidCommand(string Message) : IRequest;

public class MitVoidHandler : MitMediator.IRequestHandler<MitVoidCommand>
{
    public ValueTask<Unit> HandleAsync(MitVoidCommand request, CancellationToken cancellationToken) =>
        ValueTask.FromResult(new Unit());
}

public class MitVoidHandlerTask : MitMediator.Tasks.IRequestHandler<MitVoidCommand>
{
    public Task<Unit> Handle(MitVoidCommand request, CancellationToken cancellationToken) =>
        Task.FromResult(new Unit());
}