using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Benchmarks.EmopustaMediatR;
using Benchmarks.Mit;
using Microsoft.Extensions.DependencyInjection;
using MitMediator;

namespace Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net60)]
public class BenchmarkSendWithStreamResultAndBehaviors
{
    private MitMediator.IMediator _mitMediatr;
    private MediatR.IMediator _mediatrMediator;

    private MitMediatorStreamRequest _mitRequest = new(10);
    private MediatRStreamRequest _mediatRRequest = new(10);

    [GlobalSetup]
    public void Setup()
    {
        var services1 = new ServiceCollection();
        services1.AddMediatR(cnf => cnf.RegisterServicesFromAssembly(typeof(MitPingCommand).Assembly));
        services1.AddTransient<MediatR.IStreamPipelineBehavior<MediatRStreamRequest, int>, MediatRStreamBehavior>();
        _mediatrMediator = services1.BuildServiceProvider().GetRequiredService<MediatR.IMediator>();

        var services2 = new ServiceCollection();
        services2.AddMitMediator(typeof(MitPingCommand).Assembly);
        services1.AddTransient<IStreamPipelineBehavior<MitMediatorStreamRequest, int>, MitMediatorStreamBehavior>();
        _mitMediatr = services2.BuildServiceProvider().GetRequiredService<MitMediator.IMediator>();
    }
    
    [Benchmark(Baseline = true)]
    public async ValueTask<int> MediatR_SendRequest_ReturnResultStreamResult_UseBehaviors()
    {
        int sum = 0;
        var stream =  _mediatrMediator.CreateStream(_mediatRRequest, CancellationToken.None);
        await foreach (var value in stream)
        {
            sum+=value;
        }

        return sum;
    }
    
    [Benchmark]
    public async ValueTask<int> MitMediator_ReturnResultStreamResult_UseBehaviors()
    {
        int sum = 0;
        var stream =  _mitMediatr.CreateStream<MitMediatorStreamRequest, int>(_mitRequest, CancellationToken.None);
        await foreach (var value in stream)
        {
            sum+=value;
        }

        return sum;
    }
}