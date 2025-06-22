using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace MitMediator.Tests;

public class StreamNumbers : IStreamRequest<int>
{
    public int Count { get; }

    public StreamNumbers(int count) => Count = count;
}

public class StreamNumbersHandler : IStreamRequestHandler<StreamNumbers, int>
{
    public async IAsyncEnumerable<int> HandleAsync(
        StreamNumbers request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        for (int i = 1; i <= request.Count; i++)
        {
            yield return i;
            await Task.Delay(10, cancellationToken); // симуляция задержки
        }
    }
}

public class StreamRequestHandlerTest
{
    [Fact]
    public async Task StreamRequestHandler_ReturnsExpectedSequence()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddMitMediator(typeof(StreamNumbers).Assembly);

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        // Act
        var stream = mediator.CreateStream<StreamNumbers, int>(
            new StreamNumbers(count: 3),
            CancellationToken.None);

        var results = new List<int>();
        await foreach (var number in stream)
            results.Add(number);

        // Assert
        Assert.Equal(new[] { 1, 2, 3 }, results);
    }
}