using MediatR;

namespace Benchmarks.EmopustaMediatR;

public record MediatRPingCommand(string Message) : IRequest<string>;

public class MediatRPingHandler : IRequestHandler<MediatRPingCommand, string>
{
    public Task<string> Handle(MediatRPingCommand request, CancellationToken cancellationToken) => 
        Task.FromResult(request.Message);
}