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

        var provider = new ServiceCollection()
            .AddSingleton(handlerMock.Object)
            .AddMitMediator(typeof(VoidRequest).Assembly)
            .BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

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
            .Returns(Task.FromResult(Unit.Value));

        var provider = new ServiceCollection()
            .AddSingleton(handlerMock.Object)
            .AddMitMediator(typeof(VoidRequest).Assembly)
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
        var request = new VoidRequest();
        var handlerMock = new Mock<Tasks.IRequestHandler<VoidRequest>>();
        handlerMock
            .Setup(h => h.Handle(request, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        var provider = new ServiceCollection()
            .AddSingleton(handlerMock.Object)
            .AddMitMediator(typeof(VoidRequest).Assembly)
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
                It.IsAny<IRequestHandlerNext<PingRequest, string>>(),
                It.IsAny<CancellationToken>()))
            .Returns((PingRequest _, IRequestHandlerNext<PingRequest, string> next, CancellationToken ct) =>
            {
                executionOrder.Add("High");
                return next.InvokeAsync(request, ct);
            });

        var behaviorLow = new Mock<IPipelineBehavior<PingRequest, string>>();
        behaviorLow
            .Setup(b => b.HandleAsync(
                request,
                It.IsAny<IRequestHandlerNext<PingRequest, string>>(),
                It.IsAny<CancellationToken>()))
            .Returns((PingRequest _, IRequestHandlerNext<PingRequest, string> next, CancellationToken ct) =>
            {
                executionOrder.Add("Low");
                return next.InvokeAsync(request, ct);
            });

        var provider = new ServiceCollection()
            .AddSingleton(handlerMock.Object)
            .AddSingleton(behaviorHigh.Object)
            .AddSingleton(behaviorLow.Object)
            .AddMitMediator(typeof(VoidRequest).Assembly)
            .BuildServiceProvider();
        
        var mediator = provider.GetRequiredService<IMediator>();

        // Act
        var result = await mediator.Send<PingRequest, string>(request, default);

        // Assert
        Assert.Equal("Handled", result);
        Assert.Equal(new List<string> { "High", "Low"  }, executionOrder);
    }

    [Fact]
    public async Task Send_VoidRequest_BehaviorsOrder()
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
                It.IsAny<IRequestHandlerNext<VoidRequest, Unit>>(),
                It.IsAny<CancellationToken>()))
            .Returns((VoidRequest _, IRequestHandlerNext<VoidRequest, Unit> next, CancellationToken ct) =>
            {
                executionOrder.Add("Low");
                return next.InvokeAsync(request, ct);
            });

        var behaviorHigh = new Mock<IPipelineBehavior<VoidRequest, Unit>>();
        behaviorHigh
            .Setup(b => b.HandleAsync(
                request,
                It.IsAny<IRequestHandlerNext<VoidRequest, Unit>>(),
                It.IsAny<CancellationToken>()))
            .Returns((VoidRequest _, IRequestHandlerNext<VoidRequest, Unit> next, CancellationToken ct) =>
            {
                executionOrder.Add("High");
                return next.InvokeAsync(request, ct);
            });

        var provider = new ServiceCollection()
            .AddSingleton(handlerMock.Object)
            .AddSingleton(behaviorHigh.Object)
            .AddSingleton(behaviorLow.Object)
            .AddMitMediator(typeof(VoidRequest).Assembly)
            .BuildServiceProvider();
        
        var mediator = provider.GetRequiredService<IMediator>();


        // Act
        await mediator.Send(request, CancellationToken.None);

        // Assert
        Assert.Equal(new List<string> { "High", "Low"  }, executionOrder);
    }
}