using MitMediator;

namespace Benchmarks.Mit;

public record MitPingCommand(string Message) : IRequest<string>;


public class MitPingHandler : MitMediator.IRequestHandler<MitPingCommand, string>
{
    public ValueTask<string> HandleAsync(MitPingCommand request, CancellationToken cancellationToken)
        => ValueTask.FromResult(request.Message);
}