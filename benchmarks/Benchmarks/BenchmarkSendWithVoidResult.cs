using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Benchmarks.EmopustaMediatR;
using Benchmarks.Mit;
using Microsoft.Extensions.DependencyInjection;

namespace Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
public class BenchmarkSendWithVoidResult
{
    private MitMediator.IMediator _mitMediatr;
    private MediatR.IMediator _mediatrMediator;
    
    private MitVoidCommand _mitVoidCommand = new("Hello");
    private MediatRVoidCommand _mediatRVoidCommand = new("Hello");

    [GlobalSetup]
    public void Setup()
    {
        var services1 = new ServiceCollection();
        services1.AddMediatR(cnf => cnf.RegisterServicesFromAssembly(typeof(MitPingCommand).Assembly));
        _mediatrMediator = services1.BuildServiceProvider().GetRequiredService<MediatR.IMediator>();

        var services2 = new ServiceCollection();
        services2.AddMitMediator(typeof(MitPingCommand).Assembly);
        _mitMediatr = services2.BuildServiceProvider().GetRequiredService<MitMediator.IMediator>();
    }
    
    [Benchmark(Baseline = true)]
    public async Task MediatR_SendRequest_ReturnVoid()
    {
        await _mediatrMediator.Send(_mediatRVoidCommand, CancellationToken.None);
    }
    
    [Benchmark]
    public async ValueTask MitMediator_SendRequest_ReturnVoid()
    {
        await _mitMediatr.SendAsync(_mitVoidCommand, CancellationToken.None);
    }
}