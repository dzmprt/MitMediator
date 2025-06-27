using MediatR;

namespace Benchmarks.EmopustaMediatR;

public record MediatRVoidCommand(string Message) : IRequest<Unit>;

public class MediatRVoidHandler : IRequestHandler<MediatRVoidCommand, Unit>
{
    public Task<Unit> Handle(MediatRVoidCommand request, CancellationToken cancellationToken) => 
        Task.FromResult(Unit.Value);
}