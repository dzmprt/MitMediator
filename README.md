MitMediator
=======
![.NET 6.0](https://img.shields.io/badge/Version-.NET%206.0-informational?style=flat&logo=dotnet)
[![Build and Test](https://github.com/dzmprt/MitMediator/actions/workflows/dotnet.yml/badge.svg)](https://github.com/dzmprt/MitMediator/actions/workflows/dotnet.yml)

**MitMediator** is a lightweight and extensible mediator implementation inspired by MediatR. It supports pipeline behaviors with ordering, async handling, and seamless integration with the .NET dependency injection system

## ✨ Features

- Supports `IRequest<TResponse>` and `IRequest` (void-style)
- Custom handlers via `IRequestHandler<,>` and `IRequestHandler<>`
- Supports both `ValueTask` (for modern, efficient handlers) and `Task` (for compatibility with MediatR-style handlers)
- Enables middleware-style pipelines with `IPipelineBehavior<TRequest, TResponse>`
- Supports `INotificationHandler` with serial and parallel publishing
- Ordered execution of pipeline behaviors
- Simple registration through `AddMitMediator()` or assembly scanning

## 🚀 Getting Started

### Installation

```bash
dotnet add package MitMediator -v 6.0.0-alfa-2
```

## Example Usage

### Simple application with PingRequest, PingRequestHandler and two behaviors.

```cs
using Microsoft.Extensions.DependencyInjection;
using MitMediator;


var services = new ServiceCollection();
services
    .AddMitMediator(typeof(PingRequestHandler).Assembly)
    .AddTransient(typeof(IPipelineBehavior<,>), typeof(LowBehavior<,>))
    .AddTransient(typeof(IPipelineBehavior<,>), typeof(HeightBehavior<,>));

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
        return new ValueTask<string>("Pong result");
    }
}

public class LowBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public async ValueTask<TResponse> HandleAsync(
        TRequest request,
        Func<ValueTask<TResponse>> next,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"LowBehavior: Handling {typeof(TRequest).Name}");
        var result = await next();
        Console.WriteLine($"LowBehavior: Handled {typeof(TRequest).Name}");
        return result;
    }
}

public class HeightBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public async ValueTask<TResponse> HandleAsync(
        TRequest request,
        Func<ValueTask<TResponse>> next,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"HeightBehavior: Handling {typeof(TRequest).Name}");
        var result = await next();
        Console.WriteLine($"HeightBehavior: Handled {typeof(TRequest).Name}");
        return result;
    }
}
```

To use `Task` instead of `ValueTask`, implement the standard `IRequestHandler` interface.

### 🔁 Migrating from MediatR

You can reuse your existing handlers with minimal modifications — just update the namespaces and registration calls.

1. Add the `MitMediator` package

```bash
   dotnet add package MitMediator -v 6.0.0-alfa-2
```

2. In your request files, replace the namespace `MediatR` with `MitMediator`.
3. In your request handler files, replace the namespace `MediatR` with `MitMediator.Tasks`.
4. Update your dependency injection setup: replace .`AddMediatR(...)` with `.AddMitMediator()`.
5. If you're implementing `INotificationHandler`, use `ValueTask` instead of `Task`
5. Build and run your project — you’re all set!

MitMediator is designed to feel familiar for those coming from MediatR. Core concepts like IRequest, IRequestHandle, and pipeline behaviors are preserved — but with a cleaner interface and support for ValueTask out of the box.

### 🔍 Comparison: MitMediator vs. MediatR

| Feature                     | MitMediator                                          | MediatR                                                     |
|-----------------------------|------------------------------------------------------|-------------------------------------------------------------|
| **Return types**            | `ValueTask` (default, allocation-friendly)           | `Task` (standard async support)                             |
| **DI Registration**         | `AddMitMediator()` with optional assembly scanning   | `AddMediatR()` with assemblies explicitly specified         |
| **Extensibility**           | Designed for lightweight extension and customization | More opinionated; extensibility requires deeper integration |
| **Return types**            | `ValueTask` (default, allocation-friendly)           | `Task` (standard async support)                             |
| **Notification publishing** | Serial and parallel                                  | Only serial out of the box                                  |
| **Performance Focus**       | Async-first, zero-allocation oriented                | Flexible but not optimized for `ValueTask`                  |
| **License & Availability**  | MIT                                                  | Apache 2.0                                                  |

## 🧪 Testing

This project includes comprehensive unit tests with **100% code coverage**. All tests are included in the repository

## 📜 License

MIT


