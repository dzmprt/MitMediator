using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MitMediator.Tests;

public class DependencyInjectionTests
{
    public class SampleRequest : IRequest<string> { }

    public class SampleHandler : IHandler<SampleRequest, string>
    {
        public ValueTask<string> HandleAsync(SampleRequest request, CancellationToken cancellationToken)
            => new("response");
    }
    
    public class SampleRequest2 : IRequest<string> { }

    public class SampleRequest2Handler : Tasks.IRequestHandler<SampleRequest2, string>
    {
        public Task<string> Handle(SampleRequest2 request, CancellationToken cancellationToken)
        {
            return Task.FromResult("response");
        }
    }
    
    public class VoidRequest : IRequest { }
    
    public class VoidHandler : IHandler<VoidRequest>
    {
        public ValueTask HandleAsync(VoidRequest request, CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
        }
    }
    
    public class VoidRequest2 : IRequest { }
    
    public class VoidRequest2Handler : Tasks.IRequestHandler<VoidRequest2>
    {
        public Task Handle(VoidRequest2 request, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task AddMitMediator_RegistersMediatorAndHandlers()
    {
        // Arrange
        var services = new ServiceCollection();
        var testAssembly = typeof(SampleRequest).Assembly;

        // Act
        services.AddMitMediator(testAssembly);
        var provider = services.BuildServiceProvider();

        // Assert
        var mediator = provider.GetService<IMediator>();
        Assert.NotNull(mediator);

        var sampleRequestHandler = provider.GetRequiredService<IHandler<SampleRequest, string>>();
        Assert.NotNull(sampleRequestHandler);
        Assert.IsType<SampleHandler>(sampleRequestHandler);
        var response = await sampleRequestHandler.HandleAsync(new SampleRequest(), CancellationToken.None);
        Assert.Equal("response", response);

        var sampleRequest2Handler = provider.GetRequiredService<Tasks.IRequestHandler<SampleRequest2, string>>();
        Assert.NotNull(sampleRequest2Handler);
        Assert.IsType<SampleRequest2Handler>(sampleRequest2Handler);
        response = await sampleRequest2Handler.Handle(new SampleRequest2(), CancellationToken.None);
        Assert.Equal("response", response);

        var voidRequestHandler = provider.GetRequiredService<IHandler<VoidRequest>>();
        Assert.NotNull(voidRequestHandler);
        Assert.IsType<VoidHandler>(voidRequestHandler);
        await voidRequestHandler!.HandleAsync(new VoidRequest(), CancellationToken.None);
        
        var voidRequest2Handler = provider.GetRequiredService<Tasks.IRequestHandler<VoidRequest2>>();
        Assert.NotNull(voidRequest2Handler);
        Assert.IsType<VoidRequest2Handler>(voidRequest2Handler);
        await voidRequest2Handler!.Handle(new VoidRequest2(), CancellationToken.None);
    }
    
    [Fact]
    public void AddMitMediator_NoParams_Registers_Mediator_And_KnownHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMitMediator();
        var provider = services.BuildServiceProvider();

        // Assert
        var mediator = provider.GetService<IMediator>();
        Assert.NotNull(mediator);

        var handler = provider.GetService<IHandler<SampleRequest, string>>();
        Assert.NotNull(handler);
        Assert.IsType<SampleHandler>(handler);
    }
}