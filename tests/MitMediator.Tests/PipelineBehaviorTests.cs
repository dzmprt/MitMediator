using Microsoft.Extensions.DependencyInjection;
using MitMediator.Tasks;

namespace MitMediator.Tests;

public class PipelineBehaviorTests
{
    public class SpecificCommand : IRequest<string> { }
    public class OtherCommand : IRequest<string> { }

    public class SpecificCommandRequestHandler : IRequestHandler<SpecificCommand, string>
    {
        public ValueTask<string> HandleAsync(SpecificCommand request, CancellationToken cancellationToken)
            => new("Handled Specific");
    }

    public class OtherCommandRequestHandler : IRequestHandler<OtherCommand, string>
    {
        public ValueTask<string> HandleAsync(OtherCommand request, CancellationToken cancellationToken) 
            => new("Handled Other");
    }

    public class SpecificCommandBehavior : IPipelineBehavior<SpecificCommand, string>
    {
        public int CallCount { get; private set; }
        
        public string Response { get; private set; }

        public async ValueTask<string> HandleAsync(
            SpecificCommand request,
            Func<ValueTask<string>> next,
            CancellationToken cancellationToken)
        {
            CallCount++;
            Response = await next();
            return Response;
        }
    }
    
    [Fact]
    public async Task PipelineBehavior_AppliesOnlyToSpecificCommand()
    {
        // Arrange
        var behaviorForSpecificCommand = new SpecificCommandBehavior();

        var services = new ServiceCollection()
            .AddSingleton<IRequestHandler<SpecificCommand, string>, SpecificCommandRequestHandler>()
            .AddSingleton<IRequestHandler<OtherCommand, string>, OtherCommandRequestHandler>()
            .AddSingleton<IPipelineBehavior<SpecificCommand, string>>(behaviorForSpecificCommand)
            .AddMitMediator(typeof(SampleNotification).Assembly);

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        // Act
        var resultSpecific = await mediator.SendAsync<SpecificCommand, string>(new(), CancellationToken.None);
        var resultOther = await mediator.SendAsync<OtherCommand, string>(new(), CancellationToken.None);

        // Assert
        Assert.Equal("Handled Specific", resultSpecific);
        Assert.Equal("Handled Other", resultOther);
        Assert.Equal(1, behaviorForSpecificCommand.CallCount);
        Assert.Equal("Handled Specific", behaviorForSpecificCommand.Response);
    }
}