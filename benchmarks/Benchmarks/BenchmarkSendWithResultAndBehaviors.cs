using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Benchmarks.EmopustaMediatR;
using Benchmarks.Mit;
using Microsoft.Extensions.DependencyInjection;

namespace Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net60)]
public class BenchmarkSendWithResultAndBehaviors
{
    private MitMediator.IMediator _mitMediatr;
    private MediatR.IMediator _mediatrMediator;

    private MitPingCommand _mitRequest = new("Hello");
    private MediatRPingCommand _mediatRRequest = new("Hello");

    [GlobalSetup]
    public void Setup()
    {
        var services1 = new ServiceCollection();
        services1.AddMediatR(cnf => cnf.RegisterServicesFromAssembly(typeof(MitPingCommand).Assembly));
        services1.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(MediatRPipelineBehaviorFirst<,>));
        services1.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(MediatRPipelineBehaviorSecond<,>));
        _mediatrMediator = services1.BuildServiceProvider().GetRequiredService<MediatR.IMediator>();

        var services2 = new ServiceCollection();
        services2.AddMitMediator(typeof(MitPingCommand).Assembly);
        services1.AddTransient(typeof(MitMediator.IPipelineBehavior<,>), typeof(MitPipelineBehaviorFirst<,>));
        services1.AddTransient(typeof(MitMediator.IPipelineBehavior<,>), typeof(MitPipelineBehaviorSecond<,>));
        _mitMediatr = services2.BuildServiceProvider().GetRequiredService<MitMediator.IMediator>();
    }
    
    [Benchmark(Baseline = true)]
    public async Task<string> MediatR_SendRequest_ReturnResult_UseBehaviors()
    {
        return await _mediatrMediator.Send(_mediatRRequest, CancellationToken.None);
    }
    
    [Benchmark]
    public async ValueTask<string> MitMediator_SendRequest_ReturnResult_UseBehaviors()
    {
        return await _mitMediatr.SendAsync<MitPingCommand, string>(_mitRequest, CancellationToken.None);
    }
}