using Microsoft.Extensions.DependencyInjection;
using MitMediator;

var services = new ServiceCollection();
services
    .AddScoped<IMediator, Mediator>()
    .AddTransient<IRequestHandler<PingRequest, string>, PingRequestHandler>()
    .AddTransient<IRequestHandler<PingRequest, string>, PingRequestHandler>()
    .AddTransient<INotificationHandler<Notification>, NotificationHandler>()
    .AddTransient(typeof(IPipelineBehavior<,>), typeof(HeightBehavior<,>))
    .AddTransient(typeof(IPipelineBehavior<,>), typeof(LowBehavior<,>));

var provider = services.BuildServiceProvider();
var mediator = provider.GetRequiredService<IMediator>();

// HeightBehavior: Handling PingRequest
// LowBehavior: Handling PingRequest
// PingRequestHandler: Pong
// NotificationHandler: Notification!
// LowBehavior: Handled PingRequest
// HeightBehavior: Handled PingRequest
string result = await mediator.SendAsync<PingRequest, string>(new PingRequest(), CancellationToken.None);

Console.WriteLine(result); //Pong result

public class PingRequest : IRequest<string> { }

public class PingRequestHandler : IRequestHandler<PingRequest, string>
{
    private readonly IMediator _mediator;

    public PingRequestHandler(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    public ValueTask<string> HandleAsync(PingRequest request, CancellationToken cancellationToken)
    {
        Console.WriteLine("PingRequestHandler: Pong");
        _mediator.PublishAsync(new Notification(), cancellationToken);
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

public class Notification : INotification{}

public class NotificationHandler : INotificationHandler<Notification>
{
    public ValueTask HandleAsync(Notification notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"NotificationHandler: Notification!");
        return ValueTask.CompletedTask;
    }
}