MitMediator
=======
[![Build and Test](https://github.com/dzmprt/MitMediator/actions/workflows/dotnet.yml/badge.svg)](https://github.com/dzmprt/MitMediator/actions/workflows/dotnet.yml)
![NuGet](https://img.shields.io/nuget/v/MitMediator)
![.NET 6.0](https://img.shields.io/badge/Version-.NET%206.0-informational?style=flat&logo=dotnet)
![Coverage](https://img.shields.io/badge/coverage-100%25-brightgreen)
![NuGet Downloads](https://img.shields.io/nuget/dt/MitMediator)
![License](https://img.shields.io/github/license/dzmprt/MitMediator)


**MitMediator** is a lightweight and extensible mediator implementation inspired by MediatR. It supports pipeline behaviors with ordering, async handling, and seamless integration with the .NET dependency injection system

## ✨ Features

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
dotnet add package MitMediator -v 6.0.0-alfa-4
```

## Example Usage

### Simple application with PingRequest, PingRequestHandler and two behaviors.

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

To use `Task` instead of `ValueTask`, use `MitMediator.Tasks` namespase.

### 🔁 Migrating from MediatR

You can reuse your existing handlers with minimal modifications — just update the namespaces and registration calls.

1. Add the `MitMediator` package

```bash
   dotnet add package MitMediator -v 6.0.0-alfa-4
```

2. In your request files, replace the namespace `MediatR` with `MitMediator`.
3. In your request handler files, replace the namespace `MediatR` with `MitMediator` or `MitMediator.Tasks` (for `Task` result).
4. Update your dependency injection setup: replace .`AddMediatR(...)` with `.AddMitMediator()`
5. If you're implementing `INotificationHandler`, use `ValueTask` instead of `Task`
6. Change all `mediator.Send(request, ct)` to `mediator<TRequset, TResponse>.SendAsync(request, ct)` (or `mediator<TRequset, TResponse>.Send(request, ct)` for `Task` result)
5. Build and run your project — you’re all set!

MitMediator is designed to feel familiar for those coming from MediatR. Core concepts like IRequest, IRequestHandle, and pipeline behaviors are preserved — but with a cleaner interface and support for ValueTask out of the box.

### 🔍 Comparison: MitMediator vs. MediatR

### Performance

| Mediator    | Method                                     | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated |
|-------------|--------------------------------------------|---------:|--------:|--------:|------:|-------:|----------:|
| MediatR     | Send (return result)                       | 252.3 ns | 0.74 ns | 0.70 ns |  1.00 | 0.1836 |     384 B |
| MitMediator | SendAsync (return result)                  | 107.0 ns | 0.16 ns | 0.15 ns |  0.42 |      - |         - |
| MediatR     | Send (return result, use behaviors) | 462.7 ns | 1.35 ns | 1.26 ns |  1.00 | 0.4129 |     864 B |
| MitMediator | SendAsync (return result, use behaviors) | 107.8 ns | 0.83 ns | 0.74 ns |  0.23 |      - |         - |
| MediatR     | Send (Return void)                  | 249.1 ns | 2.24 ns | 2.10 ns |  1.00 | 0.1488 |     312 B |
| MitMediator | SendAsync (Return void)                  | 134.5 ns | 0.37 ns | 0.34 ns |  0.54 |      - |         - |
| MediatR     | Publish                                    | 296.10 ns | 1.739 ns | 1.541 ns |  1.00 | 0.2103 |     440 B |
| MitMediator | PublishAsync                                    |  81.52 ns | 0.167 ns | 0.148 ns |  0.28 |      - |         - |
| MediatR     | CreateStream (return stream, use behavior) | 1,447.0 ns | 9.20 ns | 8.16 ns |  1.00 | 0.5722 |    1200 B |
| MitMediator | CreateStream (return stream, use behavior) |   340.7 ns | 0.98 ns | 0.87 ns |  0.24 | 0.0572 |     120 B |

### Features

| Feature                     | MitMediator                                                | MediatR                                                     |
|-----------------------------|------------------------------------------------------------|-------------------------------------------------------------|
| **Return types**            | `ValueTask` (default, allocation-friendly)                 | `Task` (standard async support)                             |
| **Send methods**            | Strongly typed requests (`SendAsync<TRequest, TResponse>`) | Loosely typed requests (`Send(IRequest)`)                   |
| **DI Registration**         | `AddMitMediator()` with optional assembly scanning         | `AddMediatR()` with assemblies explicitly specified         |
| **Extensibility**           | Designed for lightweight extension and customization       | More opinionated; extensibility requires deeper integration |
| **Return types**            | `ValueTask` (default, allocation-friendly)                 | `Task` (standard async support)                             |
| **Notification publishing** | Serial and parallel                                        | Only serial out of the box                                  |
| **Performance Focus**       | Async-first, zero-allocation for `ValueTask`                          | Flexible but not optimized for `ValueTask`                  |
| **License & Availability**  | MIT                                                        | Apache 2.0                                                  |

## 🧪 Testing

This project includes comprehensive unit tests with **100% code coverage**. All tests are included in the repository

## 📜 License

MIT


