using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EFCoreSecondLevelCacheInterceptor.Tests;

[TestClass]
public class CacheAllQueriesTests
{
    [TestMethod]
    [DataRow(TestCacheProvider.BuiltInInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
    [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
    [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
    [DataRow(TestCacheProvider.FusionCache)]
    [DataRow(TestCacheProvider.StackExchangeRedis)]
    public async Task TestCacheAllQueriesWorks(TestCacheProvider cacheProvider)
        => await EFServiceProvider.RunInContextAsync(cacheProvider, LogLevel.Debug, cacheAllQueries: true,
            async (context, loggerProvider) =>
            {
                loggerProvider.ClearItems();

                var isActive = true;
                var name = "Product1";

                var list1 = await context.Products.OrderBy(product => product.ProductNumber)
                    .Where(product => product.IsActive == isActive && product.ProductName == name)
                    .ToListAsync();

                Assert.IsTrue(list1.Any());
                Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());

                var list2 = await context.Products.OrderBy(product => product.ProductNumber)
                    .Where(product => product.IsActive == isActive && product.ProductName == name)
                    .ToListAsync();

                Assert.IsTrue(list2.Any());
                Assert.AreEqual(expected: 1, loggerProvider.GetCacheHitCount());
            });

    [TestMethod]
    [DataRow(TestCacheProvider.BuiltInInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
    [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
    [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
    [DataRow(TestCacheProvider.FusionCache)]
    [DataRow(TestCacheProvider.StackExchangeRedis)]
    public async Task TestCacheAllQueriesWithNotCacheableWorks(TestCacheProvider cacheProvider)
        => await EFServiceProvider.RunInContextAsync(cacheProvider, LogLevel.Debug, cacheAllQueries: true,
            async (context, loggerProvider) =>
            {
                var isActive = true;
                var name = "Product1";

                var list1 = await context.Products.OrderBy(product => product.ProductNumber)
                    .Where(product => product.IsActive == isActive && product.ProductName == name)
                    .NotCacheable()
                    .ToListAsync();

                Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());
                Assert.IsTrue(list1.Any());

                var list2 = await context.Products.OrderBy(product => product.ProductNumber)
                    .Where(product => product.IsActive == isActive && product.ProductName == name)
                    .NotCacheable()
                    .ToListAsync();

                Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());
                Assert.IsTrue(list2.Any());
            });
}