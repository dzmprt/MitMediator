using Microsoft.Extensions.DependencyInjection;

namespace MitMediator.Tests;

public class SampleNotification : INotification
{
    public List<string> Log { get; } = new();
}

public class ForOneHandlerNotification : INotification
{
    public List<string> Log { get; } = new();
}

public class NotificationWithoutHandler : INotification
{
    public List<string> Log { get; } = new();
}

public class FirstHandler : INotificationHandler<SampleNotification>
{
    public ValueTask HandleAsync(SampleNotification notification, CancellationToken cancellationToken)
    {
        notification.Log.Add("First");
        return ValueTask.CompletedTask;
    }
}

public class SecondHandler : INotificationHandler<SampleNotification>
{
    public ValueTask HandleAsync(SampleNotification notification, CancellationToken cancellationToken)
    {
        notification.Log.Add("Second");
        return ValueTask.CompletedTask;
    }
}

public class ForOneHandlerNotificationHandler : INotificationHandler<ForOneHandlerNotification>
{
    public ValueTask HandleAsync(ForOneHandlerNotification notification, CancellationToken cancellationToken)
    {
        notification.Log.Add("Only one handler");
        return ValueTask.CompletedTask;
    }
}

public class NotificationTests
{
    [Fact]
    public async Task PublishParallelAsync_CallsWhenNoNotificationHandlers()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddMitMediator(typeof(FirstHandler).Assembly);

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var notification = new NotificationWithoutHandler();

        // Act
        await mediator.PublishParallelAsync(notification);

        // Assert
        Assert.Empty(notification.Log);
    }

    [Fact]
    public async Task PublishParallelAsync_CallsOneNotificationHandler()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddMitMediator(typeof(FirstHandler).Assembly);

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var notification = new ForOneHandlerNotification();

        // Act
        await mediator.PublishParallelAsync(notification);

        // Assert
        Assert.Contains("Only one handler", notification.Log);
        Assert.Single(notification.Log);
    }

    [Fact]
    public async Task PublishParallelAsync_CallsAllNotificationHandlers()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddMitMediator(typeof(FirstHandler).Assembly);

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var notification = new SampleNotification();

        // Act
        await mediator.PublishParallelAsync(notification);

        // Assert
        Assert.Contains("First", notification.Log);
        Assert.Contains("Second", notification.Log);
        Assert.Equal(2, notification.Log.Count);
    }

    [Fact]
    public async Task PublishAsync_CallsAllNotificationHandlers()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddMitMediator(typeof(SampleNotification).Assembly);

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var notification = new SampleNotification();

        // Act
        await mediator.PublishAsync(notification);

        // Assert
        Assert.Contains("First", notification.Log);
        Assert.Contains("Second", notification.Log);
        Assert.Equal(2, notification.Log.Count);
    }
}