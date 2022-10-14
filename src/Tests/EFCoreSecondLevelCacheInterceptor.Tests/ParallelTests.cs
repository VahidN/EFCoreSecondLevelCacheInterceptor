using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EFCoreSecondLevelCacheInterceptor.Tests;

[TestClass]
public class ParallelTests
{
    [DataTestMethod]
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
            tests.Add(() =>
                          EFServiceProvider.RunInContext(cacheProvider, LogLevel.Warning, false,
                                                         (context, debugLoggerProvider) =>
                                                         {
                                                             // run normal queries
                                                             for (var i1 = 0; i1 < loopCount; i1++)
                                                             {
                                                                 var result = context.Posts.Where(post => post.Id >= 0)
                                                                     .Take(100).ToList();
                                                             }

                                                             // cache the query result
                                                             var posts = context.Posts.Where(post => post.Id >= 0)
                                                                 .Take(100).Cacheable().ToList();

                                                             // run cached queries
                                                             for (var i2 = 0; i2 < loopCount; i2++)
                                                             {
                                                                 var result = context.Posts.Where(post => post.Id >= 0)
                                                                     .Take(100).Cacheable().ToList();
                                                             }
                                                         })
                     );
        }

        Parallel.Invoke(tests.OrderBy(a => RandomNumberProvider.Next()).ToArray());
    }
}