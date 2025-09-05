MitMediator
=======
## Fast mediator for handling requests, commands, notifications, and streams with ValueTask and ordered pipelines
[![Build and Test](https://github.com/dzmprt/MitMediator/actions/workflows/dotnet.yml/badge.svg)](https://github.com/dzmprt/MitMediator/actions/workflows/dotnet.yml)
![NuGet](https://img.shields.io/nuget/v/MitMediator)
![.NET 9.0](https://img.shields.io/badge/Version-.NET%209.0-informational?style=flat&logo=dotnet)
![Coverage](https://img.shields.io/badge/coverage-100%25-brightgreen)
![NuGet Downloads](https://img.shields.io/nuget/dt/MitMediator)
![License](https://img.shields.io/github/license/dzmprt/MitMediator)

## Features

- Supports `IRequest<TResponse>` and `IRequest` (void-style)
- Custom handlers via `IRequestHandler<,>` and `IRequestHandler<>`
- Supports both `ValueTask` (for modern, efficient handlers) and `Task` (for compatibility with MediatR-style handlers)
- Enables middleware-style pipelines using `IPipelineBehavior<TRequest, TResponse>`. A pipeline behavior can be defined for all or specific request types.
- Supports `INotificationHandler` with serial and parallel publishing
- Ordered execution of pipeline behaviors
- Simple registration through `AddMitMediator()` or assembly scanning
- Supports `IStreamRequestHandle` and `IStreamPipelineBehavior`

## 🚀 Getting Started

### Installation

```bash
dotnet add package MitMediator -v 9.0.0
```

### Example Usage

This example shows a basic setup of MitMediator that demonstrates:
* Request handling via PingRequestHandler
* Notification publishing via NotificationHandler
* Two pipeline behaviors: HeightBehavior and LowBehavior

```cs
using Microsoft.Extensions.DependencyInjection;
using MitMediator;

var services = new ServiceCollection();
services
    .AddMitMediator(typeof(PingRequestHandler).Assembly)
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
```

> To use `Task` instead of `ValueTask` for handlers, reference the MitMediator.Tasks namespace

### Migrating from MediatR

You can reuse your existing handlers with minimal modifications — just update the namespaces and registration calls

1. Add the `MitMediator` package `dotnet add package MitMediator -v 9.0.0`
2. In your request files, replace the namespace `MediatR` with `MitMediator`
3. In your request handler files, replace the namespace `MediatR` with `MitMediator` (and`MitMediator.Tasks` for `Task` result)
4. Update your dependency injection setup: replace `.AddMediatR(...)` with `.AddMitMediator()`
5. If you're implementing `INotificationHandler`, use `ValueTask` instead of `Task`
6. If you're implementing `IPipelineBehavior`, use `ValueTask` instead of `Task` and `IRequestHandlerNext<TRequest, TResponse> ` instead of  `RequestHandlerDelegate<TResponse>`. Use `next.InvokeAsync(request, cancellationToken)` for next pipe
7. For handlers with void result, use `Task<Unit>` instead of `Task` (return `Unit.Value`)
8. (Optional) Change all `mediator.Send(request, ct)` to `mediator.SendAsync<TRequset, TResponse>(request, ct)` (or `mediator.Send<TRequset, TResponse>(request, ct)` for `Task` result)
9. Build and run your project — you’re all set!

> Use `SendAsync<TRequset, TResponse>(request, ct)` for best performance or `Send(request, ct)` for backward compatibility with MediatR-style semantics

MitMediator is designed to feel familiar for those coming from MediatR. Core concepts like IRequest, IRequestHandle, and pipeline behaviors are preserved — but with a cleaner interface and support for ValueTask out of the box.

## Comparison: MitMediator vs. MediatR

### Performance

| Mediator    | Method                                     | Mean (ns) | Allocated (B) |
|-------------|--------------------------------------------|----------:|--------------:|
| MediatR     | Send (return result)                       |     90.52 |           336 |
| MitMediator | SendAsync (return result)                  | **51.81** |         **0** |
| MediatR     | Send (return result, use behaviors)        |    217.60 |           864 |
| MitMediator | SendAsync (return result, use behaviors)   | **52.20** |         **0** |
| MediatR     | Send (Return void)                         |     74.97 |           192 |
| MitMediator | SendAsync (Return void)                    | **51.85** |         **0** |
| MediatR     | Publish                                    |    156.26 |           592 |
| MitMediator | PublishAsync                               | **58.10** |        **32** |
| MediatR     | CreateStream (return stream, use behavior) |     940.0 |          1168 |
| MitMediator | CreateStream (return stream, use behavior) | **203.3** |       **112** |

### Features

| Feature                     | MitMediator                                                | MediatR                                                       |
|-----------------------------|------------------------------------------------------------|---------------------------------------------------------------|
| **Return types**            | `ValueTask` (default, allocation-friendly)                 | `Task` (standard async support)                               |
| **Send methods**            | Strongly typed requests (`SendAsync<TRequest, TResponse>`) | Loosely typed requests (`Send(request)`)                      |
| **DI Registration**         | `AddMitMediator()` with optional assembly scanning         | `AddMediatR()` with assemblies explicitly specified           |
| **Extensibility**           | Designed for lightweight extension and customization       | More opinionated; extensibility requires deeper integration   |
| **Notification publishing** | Serial and parallel                                        | Only serial out of the box                                    |
| **Performance Focus**       | Async-first, zero-allocation for `ValueTask`               | Flexible but not optimized for `ValueTask`                    |
| **License & Availability**  | MIT                                                        | Reciprocal Public License 1.5 (RPL1.5) and commercial license |

## Testing

This project includes comprehensive unit tests with **100% code coverage**. All tests are included in the repository

## 🧩 Extensions

- [MitMediator.AutoApi](https://github.com/dzmprt/MitMediator.AutoApi) - auto-registers API endpoints and generates HTTP clients from request types
- [MitMediator.AppAuthorize](https://github.com/dzmprt/MitMediator.AppAuthorize) - authentication and authorization via basic auth or JWT bearer tokens


## License

MIT


