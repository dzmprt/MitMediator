
using MediatR;

namespace Benchmarks.EmopustaMediatR;

public record MediatRSampleNotification(string Message) : INotification;

public class MediatRSampleNotificationHandler : INotificationHandler<MediatRSampleNotification>
{
    public Task Handle(MediatRSampleNotification notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

public class MediatRSampleNotificationHandler2 : INotificationHandler<MediatRSampleNotification>
{
    public Task Handle(MediatRSampleNotification notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}