using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Utils;
using Microsoft.Extensions.DependencyInjection;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Horology;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Environments;

namespace EFCoreSecondLevelCacheInterceptor.PerformanceTests
{
    class Program
    {
        static void Main(string[] args)
        {
            initDb();
            runBenchmarks();
        }

        private static void runBenchmarks()
        {
            var config = ManualConfig.Create(DefaultConfig.Instance)
                                .With(BenchmarkDotNet.Analysers.EnvironmentAnalyser.Default)
                                .With(BenchmarkDotNet.Exporters.MarkdownExporter.GitHub)
                                .With(BenchmarkDotNet.Diagnosers.MemoryDiagnoser.Default)
                                .With(StatisticColumn.Mean)
                                .With(StatisticColumn.Median)
                                .With(StatisticColumn.StdDev)
                                .With(StatisticColumn.OperationsPerSecond)
                                .With(BaselineRatioColumn.RatioMean)
                                .With(RankColumn.Arabic)
                                .With(Job.Default.With(CoreRuntime.Core31)
                                    .WithIterationCount(20)
                                    .WithInvocationCount(16)
                                    .WithIterationTime(TimeInterval.FromSeconds(1000))
                                    .WithWarmupCount(4)
                                    .WithLaunchCount(1));
            BenchmarkRunner.Run<BenchmarkTests>(config);
        }

        private static void initDb()
        {
            var serviceScope = EFServiceProvider.GetRequiredService<IServiceScopeFactory>();
            serviceScope.Initialize();
            serviceScope.SeedData();
        }
    }
}
