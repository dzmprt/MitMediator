using MitMediator;

namespace Benchmarks.Mit;

public record MitPingCommand(string Message) : IRequest<string>;


public class MitPingHandler : MitMediator.IRequestHandler<MitPingCommand, string>
{
    public ValueTask<string> HandleAsync(MitPingCommand request, CancellationToken cancellationToken)
        => ValueTask.FromResult(request.Message);
}

public class MitPingHandlerTask : MitMediator.Tasks.IRequestHandler<MitPingCommand, string>
{
    public Task<string> Handle(MitPingCommand request, CancellationToken cancellationToken) 
        => Task.FromResult(request.Message);
}