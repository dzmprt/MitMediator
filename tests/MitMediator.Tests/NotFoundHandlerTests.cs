using Microsoft.Extensions.DependencyInjection;

namespace MitMediator.Tests;

public class NotFoundHandlerTests
{
    public struct RequestWithoutHandler: IRequest<string>;
    
    public struct RequestWithoutHandlerVoid : IRequest;
    
    [Fact]
    public async Task Mediator_ThrowInvalidOperationException_IfSentRequestRequestWithoutHandler()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddMitMediator(typeof(SampleNotification).Assembly);
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        // Act & Assert
        var result = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await mediator.SendAsync<RequestWithoutHandler, string>(new RequestWithoutHandler(), CancellationToken.None)); 
        Assert.Equal("No handler for request MitMediator.Tests.NotFoundHandlerTests+RequestWithoutHandler has been registered.", result.Message);
    } 

    [Fact]
    public async Task Mediator_ThrowInvalidOperationException_IfSentRequestRequestWithoutHandlerVoid()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddMitMediator(typeof(SampleNotification).Assembly);
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        // Act & Assert
        var result = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await mediator.SendAsync(new RequestWithoutHandlerVoid(), CancellationToken.None)); 
        Assert.Equal("No handler for request MitMediator.Tests.NotFoundHandlerTests+RequestWithoutHandlerVoid has been registered.", result.Message);
    } 
}