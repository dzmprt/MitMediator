using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Benchmarks.EmopustaMediatR;
 using Benchmarks.Mit;
 using Microsoft.Extensions.DependencyInjection;
 using MitMediator;
 
 namespace Benchmarks;
 
 [MemoryDiagnoser]
 [SimpleJob(RuntimeMoniker.Net90)]
 public class BenchmarkPublishNotification
 {
     private MitMediator.IMediator _mitMediatr;
     private MediatR.IMediator _mediatrMediator;
 
     private MitSampleNotification _mitNotification = new("Hello");
     private  MediatRSampleNotification _mediatRNotification = new("Hello");
 
     [GlobalSetup]
     public void Setup()
     {
         var services1 = new ServiceCollection();
         services1.AddMediatR(cnf => cnf.RegisterServicesFromAssembly(typeof(MitPingCommand).Assembly));
         services1.AddTransient<MediatR.INotificationHandler<MediatRSampleNotification>, MediatRSampleNotificationHandler>();
 
         _mediatrMediator = services1.BuildServiceProvider().GetRequiredService<MediatR.IMediator>();
 
         var services2 = new ServiceCollection();
         services2.AddMitMediator(typeof(MitPingCommand).Assembly);
         services1.AddTransient<INotificationHandler<MitSampleNotification>, MitSampleNotificationHandler>();
         _mitMediatr = services2.BuildServiceProvider().GetRequiredService<MitMediator.IMediator>();
     }
     
         
     [Benchmark(Baseline = true)]
     public async Task MediatR_Publish()
     {
         await _mediatrMediator.Publish(_mediatRNotification, CancellationToken.None);
     }
     
     [Benchmark]
     public async ValueTask MitMediator_Publish()
     {
         await _mitMediatr.PublishAsync(_mitNotification, CancellationToken.None);
     }
 }