using Microsoft.Extensions.DependencyInjection;

namespace MitMediator.Tests;

public class PipelineBehaviorTasksTests
{
    public class MutatingCommand : IRequest<string>
    {
        public string Message { get; set; } = "Default";
    }
    
    public class MutatingBehavior : IPipelineBehavior<MutatingCommand, string>
    {
        public ValueTask<string> HandleAsync(MutatingCommand request, IRequestHandlerNext<MutatingCommand, string> next, CancellationToken cancellationToken)
        {
            request.Message = "Mutated";
            return next.InvokeAsync(request, cancellationToken);
        }
    }
    
    public class MutatingCommandRequestHandler : MitMediator.Tasks.IRequestHandler<MutatingCommand, string>
    {
        public Task<string> Handle(MutatingCommand request, CancellationToken cancellationToken)
            => Task.FromResult(request.Message);
    }
    
    public class SpecificCommand : IRequest<string>
    {
    }

    public class OtherCommand : IRequest<string>
    {
    }

    public class SpecificCommandRequestHandler : MitMediator.Tasks.IRequestHandler<SpecificCommand, string>
    {
        public Task<string> Handle(SpecificCommand request, CancellationToken cancellationToken)
            => Task.FromResult("Handled Specific");
    }

    public class OtherCommandRequestHandler : MitMediator.Tasks.IRequestHandler<OtherCommand, string>
    {
        public Task<string> Handle(OtherCommand request, CancellationToken cancellationToken)
            => Task.FromResult("Handled Other");
    }

    public class SpecificCommandBehavior : IPipelineBehavior<SpecificCommand, string>
    {
        public int CallCount { get; private set; }

        public string Response { get; private set; }
        
        public async ValueTask<string> HandleAsync(SpecificCommand request, IRequestHandlerNext<SpecificCommand, string> next, CancellationToken cancellationToken)
        {
            CallCount++;
            Response = await next.InvokeAsync(request, cancellationToken);
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
        public async ValueTask<TResponse> HandleAsync(TRequest request, IRequestHandlerNext<TRequest, TResponse> next, CancellationToken cancellationToken)
        {
            GlobalCommandBehaviorCounter.CallCount++;
            var response = await next.InvokeAsync(request, cancellationToken);
            GlobalCommandBehaviorCounter.Responses.Add(response.ToString());
            return response;
        }
    }

    [Fact]
    public async Task PipelineBehavior_CanMutateRequestField()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddSingleton<IPipelineBehavior<MutatingCommand, string>, MutatingBehavior>()
            .AddSingleton<MitMediator.Tasks.IRequestHandler<MutatingCommand, string>, MutatingCommandRequestHandler>()
            .AddMitMediator(typeof(SampleNotification).Assembly);

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        // Act
        var result = await mediator.SendAsync<MutatingCommand, string>(new MutatingCommand(), CancellationToken.None);

        // Assert
        Assert.Equal("Mutated", result);
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