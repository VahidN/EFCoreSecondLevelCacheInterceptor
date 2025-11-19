using Microsoft.Extensions.Logging;

namespace EFCoreSecondLevelCacheInterceptor.Tests;

[TestClass]
public class ParallelTests
{
    [TestMethod]
    [DataRow(TestCacheProvider.BuiltInInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
    [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
    [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
    [DataRow(TestCacheProvider.EasyCachingCoreHybrid)]
    public virtual void TestParallelQueries(TestCacheProvider cacheProvider)
    {
        var tests = new List<Action>();
        const int loopCount = 30;

        for (var i = 0; i < loopCount; i++)
        {
            tests.Add(() => EFServiceProvider.RunInContext(cacheProvider, LogLevel.Warning, cacheAllQueries: false,
                (context, debugLoggerProvider) =>
                {
                    // run normal queries
                    for (var i1 = 0; i1 < loopCount; i1++)
                    {
                        var result = context.Posts.Where(post => post.Id >= 0).Take(count: 100).ToList();
                    }

                    // cache the query result
                    var posts = context.Posts.Where(post => post.Id >= 0).Take(count: 100).Cacheable().ToList();

                    // run cached queries
                    for (var i2 = 0; i2 < loopCount; i2++)
                    {
                        var result = context.Posts.Where(post => post.Id >= 0).Take(count: 100).Cacheable().ToList();
                    }
                }));
        }

        Parallel.Invoke(tests.OrderBy(a => RandomNumberProvider.Next()).ToArray());
    }
}