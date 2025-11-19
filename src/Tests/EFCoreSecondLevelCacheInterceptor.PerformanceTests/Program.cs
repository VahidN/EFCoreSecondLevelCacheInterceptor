using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace EFCoreSecondLevelCacheInterceptor.PerformanceTests;

internal static class Program
{
    private static void Main(string[] args)
    {
        initDb();
        runBenchmarks();
    }

    private static void runBenchmarks()
    {
        var config = ManualConfig.Create(DefaultConfig.Instance);
        BenchmarkRunner.Run<BenchmarkTests>(config);
    }

    private static void initDb()
    {
        var serviceScope = EFServiceProvider.GetRequiredService<IServiceScopeFactory>();
        serviceScope.Initialize();
        serviceScope.SeedData();
    }
}