﻿// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using Benchmarks;

//BenchmarkRunner.Run<BenchmarkSendWithResult>();
//BenchmarkRunner.Run<BenchmarkSendWithResultAndBehaviors>();
//BenchmarkRunner.Run<BenchmarkSendWithVoidResult>();
//BenchmarkRunner.Run<BenchmarkPublishNotification>();
BenchmarkRunner.Run<BenchmarkSendWithStreamResultAndBehaviors>();