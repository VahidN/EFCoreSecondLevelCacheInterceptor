using BenchmarkDotNet.Attributes;

namespace EFCoreSecondLevelCacheInterceptor.PerformanceTests;

public class BenchmarkTests
{
    private int _count;

    [Benchmark(Baseline = true)]
    public void RunQueryDirectly()
        => EFServiceProvider.RunInContext(db =>
        {
            var products = db.Products.Where(x => x.ProductId > 0).ToList();
            _count = products.Count;
        });

    [Benchmark]
    public void RunCacheableQueryWithMicrosoftMemoryCache()
        => EFServiceProvider.RunInContext(db =>
        {
            var products = db.Products.Where(x => x.ProductId > 0).Cacheable().ToList();
            _count = products.Count;
        });

    [GlobalCleanup] public void GlobalCleanup() => Console.WriteLine($"_count: {_count}");
}