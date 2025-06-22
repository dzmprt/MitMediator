using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace MitMediator.Tests;

public class StreamPipelineBehaviorTests
{
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
            Func<IAsyncEnumerable<int>> next,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var item in next().WithCancellation(cancellationToken))
            {
                yield return item * Multiplier;
            }
        }
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
        await foreach (var item in mediator.CreateStream<StreamQuery, int>(request))
            results.Add(item);

        // Assert
        Assert.Equal(expected, results);
    }
}