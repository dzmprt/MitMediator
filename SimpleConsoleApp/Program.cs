// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using MitMediator;


var services = new ServiceCollection();
services
    .AddMitMediator(typeof(PingRequestHandler).Assembly)
    .AddTransient(typeof(IPipelineBehavior<,>), typeof(HeightBehavior<,>))
    .AddTransient(typeof(IPipelineBehavior<,>), typeof(LowBehavior<,>));

var provider = services.BuildServiceProvider();
var mediator = provider.GetRequiredService<IMediator>();

//HeightBehavior: Handling PingRequest
//LowBehavior: Handling PingRequest
//PingRequestHandler: Pong
//LowBehavior: Handled PingRequest
//HeightBehavior: Handled PingRequest
string result = await mediator.SendAsync<PingRequest, string>(new PingRequest(), CancellationToken.None);

Console.WriteLine(result); //Pong result

public class PingRequest : IRequest<string> { }

public class PingRequestHandler : IRequestHandler<PingRequest, string>
{
    public ValueTask<string> HandleAsync(PingRequest request, CancellationToken cancellationToken)
    {
        Console.WriteLine("PingRequestHandler: Pong");
        return ValueTask.FromResult("Pong result");
    }
}

public class LowBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public async ValueTask<TResponse> HandleAsync(TRequest request, IRequestHandlerNext<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
        Console.WriteLine($"LowBehavior: Handling {typeof(TRequest).Name}");
        var result = await next.InvokeAsync(request, cancellationToken);
        Console.WriteLine($"LowBehavior: Handled {typeof(TRequest).Name}");
        return result;
    }
}

public class HeightBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public async ValueTask<TResponse> HandleAsync(TRequest request, IRequestHandlerNext<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
        Console.WriteLine($"HeightBehavior: Handling {typeof(TRequest).Name}");
        var result = await next.InvokeAsync(request, cancellationToken);
        Console.WriteLine($"HeightBehavior: Handled {typeof(TRequest).Name}");
        return result;
    }
}