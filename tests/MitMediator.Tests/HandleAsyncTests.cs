using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace MitMediator.Tests;

public class HandleAsyncTests
{
    public class PingRequestValueTask : IRequest<string>
    {
    }

    public class PongRequestValueTask : IRequest
    {
    }

    public class VoidRequestValueTask : IRequest
    {
    }

    [Fact]
    public async Task SendAsync_TRequestTResponse_InvokesHandler()
    {
        // Arrange
        var request = new PingRequestValueTask();
        var expected = "Pong";

        var handlerMock = new Mock<IRequestHandler<PingRequestValueTask, string>>();
        handlerMock
            .Setup(h => h.HandleAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var provider = new ServiceCollection()
            .AddSingleton(handlerMock.Object)
            .AddMitMediator(typeof(VoidRequestValueTask).Assembly)
            .BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        // Act
        var result = await mediator.SendAsync<PingRequestValueTask, string>(request, default);

        // Assert
        Assert.Equal(expected, result);
        handlerMock.Verify(h => h.HandleAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_TRequest_InvokesHandler()
    {
        // Arrange
        var request = new PongRequestValueTask();

        var handlerMock = new Mock<IRequestHandler<PongRequestValueTask, Unit>>();
        handlerMock
            .Setup(h => h.HandleAsync(request, It.IsAny<CancellationToken>()))
            .Returns(ValueTask.FromResult(Unit.Value));

        var provider = new ServiceCollection()
            .AddSingleton(handlerMock.Object)
            .AddMitMediator(typeof(VoidRequestValueTask).Assembly)
            .BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        // Act
        await mediator.SendAsync(request, default);

        // Assert
        handlerMock.Verify(h => h.HandleAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_VoidRequest_InvokesHandler()
    {
        // Arrange
        var request = new VoidRequestValueTask();
        var handlerMock = new Mock<IRequestHandler<VoidRequestValueTask, Unit>>();
        handlerMock
            .Setup(h => h.HandleAsync(request, It.IsAny<CancellationToken>()))
            .Returns(ValueTask.FromResult(Unit.Value));

        var provider = new ServiceCollection()
            .AddSingleton(handlerMock.Object)
            .AddMitMediator(typeof(VoidRequestValueTask).Assembly)
            .BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        // Act
        await mediator.SendAsync(request, CancellationToken.None);

        // Assert
        handlerMock.Verify(h => h.HandleAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_BehaviorsOrder()
    {
        // Arrange
        var request = new PingRequestValueTask();
        var executionOrder = new List<string>();

        var handlerMock = new Mock<IRequestHandler<PingRequestValueTask, string>>();
        handlerMock
            .Setup(h => h.HandleAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Handled");

        var behaviorHigh = new Mock<IPipelineBehavior<PingRequestValueTask, string>>();
        behaviorHigh
            .Setup(b => b.HandleAsync(
                request,
                It.IsAny<IRequestHandlerNext<PingRequestValueTask, string>>(),
                It.IsAny<CancellationToken>()))
            .Returns((PingRequestValueTask _, IRequestHandlerNext<PingRequestValueTask, string > next, CancellationToken ct) =>
            {
                executionOrder.Add("High");
                return next.InvokeAsync(request, ct);
            });
        
        var behaviorLow = new Mock<IPipelineBehavior<PingRequestValueTask, string>>();
        behaviorLow
            .Setup(b => b.HandleAsync(
                request,
                It.IsAny<IRequestHandlerNext<PingRequestValueTask, string >>(),
                It.IsAny<CancellationToken>()))
            .Returns((PingRequestValueTask _, IRequestHandlerNext<PingRequestValueTask, string > next, CancellationToken ct) =>
            {
                executionOrder.Add("Low");
                return next.InvokeAsync(request, ct);
            });

        var provider = new ServiceCollection()
            .AddSingleton(handlerMock.Object)
            .AddSingleton(behaviorHigh.Object)
            .AddSingleton(behaviorLow.Object)
            .AddMitMediator(typeof(VoidRequestValueTask).Assembly)
            .BuildServiceProvider();
        
        var mediator = provider.GetRequiredService<IMediator>();


        // Act
        var result = await mediator.SendAsync<PingRequestValueTask, string>(request, CancellationToken.None);

        // Assert
        Assert.Equal("Handled", result);
        Assert.Equal(new List<string> { "High", "Low" }, executionOrder);
    }

    [Fact]
    public async Task SendAsync_VoidRequest_BehaviorsOrder()
    {
        // Arrange
        var request = new VoidRequestValueTask();
        var executionOrder = new List<string>();

        var handlerMock = new Mock<IRequestHandler<VoidRequestValueTask, Unit>>();
        handlerMock
            .Setup(h => h.HandleAsync(request, It.IsAny<CancellationToken>()));

        var behaviorLow = new Mock<IPipelineBehavior<VoidRequestValueTask, Unit>>();
        behaviorLow
            .Setup(b => b.HandleAsync(
                request,
                It.IsAny<IRequestHandlerNext<VoidRequestValueTask, Unit >>(),
                It.IsAny<CancellationToken>()))
            .Returns((VoidRequestValueTask _, IRequestHandlerNext<VoidRequestValueTask, Unit > next, CancellationToken ct) =>
            {
                executionOrder.Add("Low");
                return next.InvokeAsync(request, ct);
            });

        var behaviorHigh = new Mock<IPipelineBehavior<VoidRequestValueTask, Unit>>();
        behaviorHigh
            .Setup(b => b.HandleAsync(
                request,
                It.IsAny<IRequestHandlerNext<VoidRequestValueTask, Unit>>(),
                It.IsAny<CancellationToken>()))
            .Returns((VoidRequestValueTask _, IRequestHandlerNext<VoidRequestValueTask, Unit > next, CancellationToken ct) =>
            {
                executionOrder.Add("High");
                return next.InvokeAsync(request, ct);
            });

        var provider = new ServiceCollection()
            .AddSingleton(handlerMock.Object)
            .AddSingleton(handlerMock.Object)
            .AddSingleton(behaviorHigh.Object)
            .AddSingleton(behaviorLow.Object)
            .AddMitMediator(typeof(VoidRequestValueTask).Assembly)
            .BuildServiceProvider();
        
        var mediator = provider.GetRequiredService<IMediator>();


        // Act
        await mediator.SendAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(new List<string> { "High", "Low"  }, executionOrder);
    }
}