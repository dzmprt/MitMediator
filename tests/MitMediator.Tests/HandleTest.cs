using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace MitMediator.Tests;

public class HandleTest
{
    public class PingRequestTasks : IRequest<string> { }

    public class PongRequestTasks : IRequest { }
    
    public class VoidRequestTasks : IRequest { }

    [Fact]
    public async Task Send_TRequestTResponse_InvokesHandler()
    {
        // Arrange
        var request = new PingRequestTasks();
        var expected = "Pong";

        var handlerMock = new Mock<MitMediator.Tasks.IRequestHandler<PingRequestTasks, string>>();
        handlerMock
            .Setup(h => h.Handle(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var provider = new ServiceCollection()
            .AddSingleton(handlerMock.Object)
            .AddMitMediator(typeof(VoidRequestTasks).Assembly)
            .BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        // Act
        var result = await mediator.Send<PingRequestTasks, string>(request, default);

        // Assert
        Assert.Equal(expected, result);
        handlerMock.Verify(h => h.Handle(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Send_TRequest_InvokesHandler()
    {
        // Arrange
        var request = new PongRequestTasks();

        var handlerMock = new Mock<MitMediator.Tasks.IRequestHandler<PongRequestTasks>>();
        handlerMock
            .Setup(h => h.Handle(request, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        var provider = new ServiceCollection()
            .AddSingleton(handlerMock.Object)
            .AddMitMediator(typeof(VoidRequestTasks).Assembly)
            .BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        // Act
        await mediator.Send(request, CancellationToken.None);

        // Assert
        handlerMock.Verify(h => h.Handle(request, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Send_VoidRequest_InvokesHandler()
    {
        
        // Arrange
        var request = new VoidRequestTasks();
        var handlerMock = new Mock<MitMediator.Tasks.IRequestHandler<VoidRequestTasks>>();
        handlerMock
            .Setup(h => h.Handle(request, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        var provider = new ServiceCollection()
            .AddSingleton(handlerMock.Object)
            .AddMitMediator(typeof(VoidRequestTasks).Assembly)
            .BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        // Act
        await mediator.Send(request, CancellationToken.None);

        // Assert
        handlerMock.Verify(h => h.Handle(request, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Send_BehaviorsOrder()
    {
        // Arrange
        var request = new PingRequestTasks();
        var executionOrder = new List<string>();

        var handlerMock = new Mock<MitMediator.Tasks.IRequestHandler<PingRequestTasks, string>>();
        handlerMock
            .Setup(h => h.Handle(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Handled");
        
        var behaviorHigh = new Mock<IPipelineBehavior<PingRequestTasks, string>>();
        behaviorHigh
            .Setup(b => b.HandleAsync(
                request,
                It.IsAny<IRequestHandlerNext<PingRequestTasks, string>>(),
                It.IsAny<CancellationToken>()))
            .Returns((PingRequestTasks _, IRequestHandlerNext<PingRequestTasks, string> next, CancellationToken ct) =>
            {
                executionOrder.Add("High");
                return next.InvokeAsync(request, ct);
            });

        var behaviorLow = new Mock<IPipelineBehavior<PingRequestTasks, string>>();
        behaviorLow
            .Setup(b => b.HandleAsync(
                request,
                It.IsAny<IRequestHandlerNext<PingRequestTasks, string>>(),
                It.IsAny<CancellationToken>()))
            .Returns((PingRequestTasks _, IRequestHandlerNext<PingRequestTasks, string> next, CancellationToken ct) =>
            {
                executionOrder.Add("Low");
                return next.InvokeAsync(request, ct);
            });

        var provider = new ServiceCollection()
            .AddSingleton(handlerMock.Object)
            .AddSingleton(behaviorHigh.Object)
            .AddSingleton(behaviorLow.Object)
            .AddMitMediator(typeof(VoidRequestTasks).Assembly)
            .BuildServiceProvider();
        
        var mediator = provider.GetRequiredService<IMediator>();

        // Act
        var result = await mediator.Send<PingRequestTasks, string>(request, default);

        // Assert
        Assert.Equal("Handled", result);
        Assert.Equal(new List<string> { "High", "Low"  }, executionOrder);
    }

    [Fact]
    public async Task Send_VoidRequest_BehaviorsOrder()
    {
        // Arrange
        var request = new VoidRequestTasks();
        var executionOrder = new List<string>();

        var handlerMock = new Mock<MitMediator.Tasks.IRequestHandler<VoidRequestTasks>>();
        handlerMock
            .Setup(h => h.Handle(request, It.IsAny<CancellationToken>()));

        var behaviorLow = new Mock<IPipelineBehavior<VoidRequestTasks, Unit>>();
        behaviorLow
            .Setup(b => b.HandleAsync(
                request,
                It.IsAny<IRequestHandlerNext<VoidRequestTasks, Unit>>(),
                It.IsAny<CancellationToken>()))
            .Returns((VoidRequestTasks _, IRequestHandlerNext<VoidRequestTasks, Unit> next, CancellationToken ct) =>
            {
                executionOrder.Add("Low");
                return next.InvokeAsync(request, ct);
            });

        var behaviorHigh = new Mock<IPipelineBehavior<VoidRequestTasks, Unit>>();
        behaviorHigh
            .Setup(b => b.HandleAsync(
                request,
                It.IsAny<IRequestHandlerNext<VoidRequestTasks, Unit>>(),
                It.IsAny<CancellationToken>()))
            .Returns((VoidRequestTasks _, IRequestHandlerNext<VoidRequestTasks, Unit> next, CancellationToken ct) =>
            {
                executionOrder.Add("High");
                return next.InvokeAsync(request, ct);
            });

        var provider = new ServiceCollection()
            .AddSingleton(handlerMock.Object)
            .AddSingleton(behaviorHigh.Object)
            .AddSingleton(behaviorLow.Object)
            .AddMitMediator(typeof(VoidRequestTasks).Assembly)
            .BuildServiceProvider();
        
        var mediator = provider.GetRequiredService<IMediator>();


        // Act
        await mediator.Send(request, CancellationToken.None);

        // Assert
        Assert.Equal(new List<string> { "High", "Low"  }, executionOrder);
    }
}