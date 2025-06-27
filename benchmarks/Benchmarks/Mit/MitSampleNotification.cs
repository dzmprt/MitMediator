using MitMediator;

namespace Benchmarks.Mit;

public record MitSampleNotification(string Message) : INotification;


public class MitSampleNotificationHandler : INotificationHandler<MitSampleNotification>
{
    public ValueTask HandleAsync(MitSampleNotification notification, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }
}

public class MitSampleNotificationHandler2 : INotificationHandler<MitSampleNotification>
{
    public ValueTask HandleAsync(MitSampleNotification notification, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }
}