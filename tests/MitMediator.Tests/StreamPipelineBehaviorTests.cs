using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Linq;

namespace MitMediator.Tests;

public class StreamPipelineBehaviorTests
{
    public class MyStreamRequest : IStreamRequest<string>
    {
    }
    
    public class StreamQuery : IStreamRequest<int>
    {
        public int Count { get; set; }
    }

    public class StreamQueryHandler : IStreamRequestHandler<StreamQuery, int>
    {
        public async IAsyncEnumerable<int> HandleAsync(StreamQuery request, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            for (var i = 0; i < request.Count; i++)
            {
                yield return i;
                await Task.Delay(10, cancellationToken);
            }
        }
    }
    
    public class MultiplyBehavior : IStreamPipelineBehavior<StreamQuery, int>
    {
        public static int Multiplier { get; set; } = 10;

        public async IAsyncEnumerable<int> HandleAsync(
            StreamQuery request,
            IAsyncEnumerable<int> nextPipe,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var item in nextPipe.WithCancellation(cancellationToken))
            {
                yield return item * Multiplier;
            }
        }
    }
    
    public class AsyncEnumerableStub<T> : IAsyncEnumerable<T>
    {
        private readonly IAsyncEnumerable<T> _inner;
        private readonly Action<T> _onItem;

        public AsyncEnumerableStub(IAsyncEnumerable<T> inner, Action<T> onItem)
        {
            _inner = inner;
            _onItem = onItem;
        }

        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            await foreach (var item in _inner.WithCancellation(cancellationToken))
            {
                _onItem?.Invoke(item);
                yield return item;
            }
        }
    }
    static async IAsyncEnumerable<string> HandlerResults()
    {
        yield return "A";
        yield return "B";
    }
    
    [Fact]
    public async Task StreamPipeline_Executes_In_Correct_Order()
    {
        // Arrange
        var callOrder = new List<string>();

        var behavior1 = new Mock<IStreamPipelineBehavior<MyStreamRequest, string>>();
        var behavior2 = new Mock<IStreamPipelineBehavior<MyStreamRequest, string>>();
        var handlerResults = HandlerResults();

        var request = new MyStreamRequest();
        
        IAsyncEnumerable<string> FinalHandler() => new AsyncEnumerableStub<string>(handlerResults, s =>
        {
            callOrder.Add($"handler:{s}");
        });

        behavior1
            .Setup(b => b.HandleAsync(request, It.IsAny<IAsyncEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .Returns((MyStreamRequest r, IAsyncEnumerable<string> next, CancellationToken ct) =>
            {
                callOrder.Add("behavior1:start");
                return new AsyncEnumerableStub<string>(next, s =>
                {
                    callOrder.Add($"behavior1:{s}");
                });
            });

        behavior2
            .Setup(b => b.HandleAsync(request, It.IsAny<IAsyncEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .Returns((MyStreamRequest r, IAsyncEnumerable<string> next, CancellationToken ct) =>
            {
                callOrder.Add("behavior2:start");
                return new AsyncEnumerableStub<string>(next, s =>
                {
                    callOrder.Add($"behavior2:{s}");
                });
            });
        
        var chain = behavior2.Object.HandleAsync(
            request,
            behavior1.Object.HandleAsync(request, FinalHandler(), default),
            default);

        await foreach (var item in chain)
        {
        }

        // Assert
        Assert.Equal(new[]
        {
            "behavior1:start",
            "behavior2:start",
            "handler:A",
            "behavior1:A",
            "behavior2:A",
            "handler:B",
            "behavior1:B",
            "behavior2:B"
        }, callOrder.ToArray());
    }
    
    [Fact]
    public async Task StreamPipelineBehavior_AppliesTransformation()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddMitMediator(typeof(StreamQueryHandler).Assembly)
            .AddSingleton<IStreamRequestHandler<StreamQuery, int>, StreamQueryHandler>()
            .AddSingleton<IStreamPipelineBehavior<StreamQuery, int>, MultiplyBehavior>();

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var request = new StreamQuery { Count = 3 };
        var expected = new[] { 0, 10, 20 };

        // Act
        var results = new List<int>();
        await foreach (var item in mediator.CreateStream<StreamQuery, int>(request, CancellationToken.None))
            results.Add(item);

        // Assert
        Assert.Equal(expected, results);
    }
}