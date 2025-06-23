using MediatR;

namespace Benchmarks.EmopustaMediatR;

public record MediatRVoidCommand(string Message) : IRequest;

public class MediatRVoidHandler : IRequestHandler<MediatRVoidCommand>
{
    public Task Handle(MediatRVoidCommand request, CancellationToken cancellationToken) => 
        Task.CompletedTask;
}