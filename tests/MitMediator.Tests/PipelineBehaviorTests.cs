using Microsoft.Extensions.DependencyInjection;
using MitMediator.Tasks;

namespace MitMediator.Tests;

public class PipelineBehaviorTests
{
    public class SpecificCommand : IRequest<string>
    {
    }

    public class OtherCommand : IRequest<string>
    {
    }

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
    
    public static class GlobalCommandBehaviorCounter
    {
        public static int CallCount { get;  set; }

        public static List<string> Responses { get; set; } = new();
    }
    
    public class GlobalCommandBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        public async ValueTask<TResponse> HandleAsync(TRequest request, Func<ValueTask<TResponse>> next, CancellationToken cancellationToken)
        {
            GlobalCommandBehaviorCounter.CallCount++;
            var response = await next();
            GlobalCommandBehaviorCounter.Responses.Add(response.ToString());
            return response;
        }
    }

    [Fact]
    public async Task PipelineBehavior_SpecificCommandBehavior()
    {
        // Arrange
        var behaviorForSpecificCommand = new SpecificCommandBehavior();
        var services = new ServiceCollection()
            .AddSingleton<IPipelineBehavior<SpecificCommand, string>>(behaviorForSpecificCommand)
            .AddMitMediator(typeof(SampleNotification).Assembly);

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        // Act
        var resultSpecific = await mediator.SendAsync<SpecificCommand, string>(new SpecificCommand(), CancellationToken.None);
        var resultOther = await mediator.SendAsync<OtherCommand, string>(new OtherCommand(), CancellationToken.None);

        // Assert
        Assert.Equal("Handled Specific", resultSpecific);
        Assert.Equal("Handled Other", resultOther);
        Assert.Equal(1, behaviorForSpecificCommand.CallCount);
        Assert.Equal("Handled Specific", behaviorForSpecificCommand.Response);
    }

    [Fact]
    public async Task PipelineBehavior_GlobalCommandBehavior()
    {
        // Arrange
        var behaviorForSpecificCommand = new SpecificCommandBehavior();
        var services = new ServiceCollection()
            .AddSingleton<IPipelineBehavior<SpecificCommand, string>>(behaviorForSpecificCommand)
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(GlobalCommandBehavior<,>))
            .AddMitMediator(typeof(SampleNotification).Assembly);

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        // Act
        var resultSpecific = await mediator.SendAsync<SpecificCommand, string>(new SpecificCommand(), CancellationToken.None);
        var resultOther = await mediator.SendAsync<OtherCommand, string>(new OtherCommand(), CancellationToken.None);

        // Assert
        Assert.Equal("Handled Specific", resultSpecific);
        Assert.Equal("Handled Other", resultOther);
        Assert.Equal(1, behaviorForSpecificCommand.CallCount);
        Assert.Equal(2, GlobalCommandBehaviorCounter.CallCount);
        Assert.Equal("Handled Specific", behaviorForSpecificCommand.Response);
        Assert.Equal(new List<string> { "Handled Specific", "Handled Other" }, GlobalCommandBehaviorCounter.Responses);
    }
}