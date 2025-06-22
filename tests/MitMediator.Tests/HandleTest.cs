using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace MitMediator.Tests;

public class HandleTest
{
    public class PingRequest : IRequest<string> { }

    public class PongRequest : IRequest { }
    
    public class VoidRequest : IRequest { }

    [Fact]
    public async ValueTask Send_TRequestTResponse_InvokesHandler()
    {
        // Arrange
        var request = new PingRequest();
        var expected = "Pong";

        var handlerMock = new Mock<Tasks.IRequestHandler<PingRequest, string>>();
        handlerMock
            .Setup(h => h.Handle(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var serviceProvider = new ServiceCollection()
            .AddSingleton(handlerMock.Object)
            .BuildServiceProvider();

        var mediator = new Mediator(serviceProvider);

        // Act
        var result = await mediator.Send<PingRequest, string>(request, default);

        // Assert
        Assert.Equal(expected, result);
        handlerMock.Verify(h => h.Handle(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async ValueTask Send_TRequest_InvokesHandler()
    {
        // Arrange
        var request = new PongRequest();

        var handlerMock = new Mock<Tasks.IRequestHandler<PongRequest>>();
        handlerMock
            .Setup(h => h.Handle(request, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var serviceProvider = new ServiceCollection()
            .AddSingleton(handlerMock.Object)
            .BuildServiceProvider();

        var mediator = new Mediator(serviceProvider);

        // Act
        await mediator.Send(request, CancellationToken.None);

        // Assert
        handlerMock.Verify(h => h.Handle(request, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Send_VoidRequest_InvokesHandler()
    {
        
        // Arrange
        var request = new VoidRequest();
        var handlerMock = new Mock<Tasks.IRequestHandler<VoidRequest>>();
        handlerMock
            .Setup(h => h.Handle(request, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var provider = new ServiceCollection()
            .AddSingleton(handlerMock.Object)
            .BuildServiceProvider();

        var mediator = new Mediator(provider);

        // Act
        await mediator.Send(request, CancellationToken.None);

        // Assert
        handlerMock.Verify(h => h.Handle(request, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Send_BehaviorsRespectOrder()
    {
        // Arrange
        var request = new PingRequest();
        var executionOrder = new List<string>();

        var handlerMock = new Mock<Tasks.IRequestHandler<PingRequest, string>>();
        handlerMock
            .Setup(h => h.Handle(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Handled");
        
        var behaviorHigh = new Mock<IPipelineBehavior<PingRequest, string>>();
        behaviorHigh
            .Setup(b => b.HandleAsync(
                request,
                It.IsAny<Func<ValueTask<string>>>(),
                It.IsAny<CancellationToken>()))
            .Returns((PingRequest _, Func<ValueTask<string>> next, CancellationToken _) =>
            {
                executionOrder.Add("High");
                return next();
            });

        var behaviorLow = new Mock<IPipelineBehavior<PingRequest, string>>();
        behaviorLow
            .Setup(b => b.HandleAsync(
                request,
                It.IsAny<Func<ValueTask<string>>>(),
                It.IsAny<CancellationToken>()))
            .Returns((PingRequest _, Func<ValueTask<string>> next, CancellationToken _) =>
            {
                executionOrder.Add("Low");
                return next();
            });

        var provider = new ServiceCollection()
            .AddSingleton(handlerMock.Object)
            .AddSingleton(behaviorHigh.Object)
            .AddSingleton(behaviorLow.Object)
            .BuildServiceProvider();

        var mediator = new Mediator(provider);

        // Act
        var result = await mediator.Send<PingRequest, string>(request, default);

        // Assert
        Assert.Equal("Handled", result);
        Assert.Equal(new List<string> { "High", "Low"  }, executionOrder);
    }

    [Fact]
    public async Task Send_VoidRequest_BehaviorsRespectOrder()
    {
        // Arrange
        var request = new VoidRequest();
        var executionOrder = new List<string>();

        var handlerMock = new Mock<Tasks.IRequestHandler<VoidRequest>>();
        handlerMock
            .Setup(h => h.Handle(request, It.IsAny<CancellationToken>()));

        var behaviorLow = new Mock<IPipelineBehavior<VoidRequest, Unit>>();
        behaviorLow
            .Setup(b => b.HandleAsync(
                request,
                It.IsAny<Func<ValueTask<Unit>>>(),
                It.IsAny<CancellationToken>()))
            .Returns((VoidRequest _, Func<ValueTask<Unit>> next, CancellationToken _) =>
            {
                executionOrder.Add("Low");
                return next();
            });

        var behaviorHigh = new Mock<IPipelineBehavior<VoidRequest, Unit>>();
        behaviorHigh
            .Setup(b => b.HandleAsync(
                request,
                It.IsAny<Func<ValueTask<Unit>>>(),
                It.IsAny<CancellationToken>()))
            .Returns((VoidRequest _, Func<ValueTask<Unit>> next, CancellationToken _) =>
            {
                executionOrder.Add("High");
                return next();
            });

        var provider = new ServiceCollection()
            .AddSingleton(handlerMock.Object)
            .AddSingleton(behaviorHigh.Object)
            .AddSingleton(behaviorLow.Object)
            .BuildServiceProvider();

        var mediator = new Mediator(provider);

        // Act
        await mediator.Send(request, CancellationToken.None);

        // Assert
        Assert.Equal(new List<string> { "High", "Low"  }, executionOrder);
    }
}