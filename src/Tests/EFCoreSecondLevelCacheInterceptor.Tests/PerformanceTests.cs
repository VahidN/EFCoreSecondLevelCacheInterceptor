using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EFCoreSecondLevelCacheInterceptor.Tests
{
    [TestClass]
    public class PerformanceTests
    {
        [DataTestMethod]
        [DataRow(TestCacheProvider.BuiltInInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
        public void PerformanceTest(TestCacheProvider cacheProvider)
        {
            const decimal loopCount = 1000;

            EFServiceProvider.RunInContext(cacheProvider, LogLevel.Warning, false, (context, debugLoggerProvider) =>
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();

                // run normal queries
                for (int i = 0; i < loopCount; i++)
                {
                    var result = context.Posts.Where(post => post.Id >= 0).Take(100).ToList();
                }

                var uncachedTimeSpan = watch.Elapsed;

                // cache the query result
                var posts = context.Posts.Where(post => post.Id >= 0).Take(100).Cacheable().ToList();

                watch.Restart();

                // run cached queries
                for (int i = 0; i < loopCount; i++)
                {
                    var result = context.Posts.Where(post => post.Id >= 0).Take(100).Cacheable().ToList();
                }

                var cachedTimeSpan = watch.Elapsed;

                var message = $"Average database query duration [+{TimeSpan.FromTicks((long)(uncachedTimeSpan.Ticks / loopCount))}].\n" +
                $"Average cache query duration [+{TimeSpan.FromTicks((long)(cachedTimeSpan.Ticks / loopCount))}].\n" +
                $"Cached queries are x{(uncachedTimeSpan.Ticks / (decimal)cachedTimeSpan.Ticks) - 1:N2} times faster.";

                Assert.IsTrue(uncachedTimeSpan > cachedTimeSpan, message);
            });
        }
    }
}