using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EFCoreSecondLevelCacheInterceptor.Tests;

[TestClass]
public class SecondLevelCacheInterceptorAsyncTests
{
    [TestMethod]
    [DataRow(TestCacheProvider.BuiltInInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
    [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
    [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
    public async Task TestSecondLevelCacheUsingAsyncMethodsDoesNotHitTheDatabase(TestCacheProvider cacheProvider)
        => await EFServiceProvider.RunInContextAsync(cacheProvider, LogLevel.Debug, cacheAllQueries: false,
            async (context, loggerProvider) =>
            {
                var isActive = true;
                var name = "Product1";

                var list1 = await context.Products.OrderBy(product => product.ProductNumber)
                    .Where(product => product.IsActive == isActive && product.ProductName == name)
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .ToListAsync();

                Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());
                Assert.IsTrue(list1.Any());

                var list2 = await context.Products.OrderBy(product => product.ProductNumber)
                    .Where(product => product.IsActive == isActive && product.ProductName == name)
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .ToListAsync();

                Assert.AreEqual(expected: 1, loggerProvider.GetCacheHitCount());
                Assert.IsTrue(list2.Any());

                var list3 = await context.Products.OrderBy(product => product.ProductNumber)
                    .Where(product => product.IsActive == isActive && product.ProductName == name)
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .ToListAsync();

                Assert.AreEqual(expected: 1, loggerProvider.GetCacheHitCount());
                Assert.IsTrue(list3.Any());

                var list4 = await context.Products.OrderBy(product => product.ProductNumber)
                    .Where(product => product.IsActive == isActive && product.ProductName == "Product2")
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .ToListAsync();

                Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());
                Assert.IsTrue(list4.Any());

                var product1 = await context.Products.OrderBy(product => product.ProductNumber)
                    .Where(product => product.IsActive == isActive && product.ProductName == "Product2")
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .FirstOrDefaultAsync();

                Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());
                Assert.IsNotNull(product1);
            });

    [TestMethod]
    [DataRow(TestCacheProvider.BuiltInInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
    [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
    [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
    public async Task TestSecondLevelCacheUsingDifferentAsyncMethods(TestCacheProvider cacheProvider)
        => await EFServiceProvider.RunInContextAsync(cacheProvider, LogLevel.Debug, cacheAllQueries: false,
            async (context, loggerProvider) =>
            {
                var isActive = true;
                var name = "Product3";

                var list1 = await context.Products.OrderBy(product => product.ProductNumber)
                    .Where(product => product.IsActive == isActive && product.ProductName == name)
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .ToListAsync();

                Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());
                Assert.IsTrue(list1.Any());

                var count = await context.Products.OrderBy(product => product.ProductNumber)
                    .Where(product => product.IsActive == isActive && product.ProductName == name)
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .CountAsync();

                Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());
                Assert.IsGreaterThan(lowerBound: 0, count);

                var product1 = await context.Products.OrderBy(product => product.ProductNumber)
                    .Where(product => product.IsActive == isActive && product.ProductName == name)
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .FirstOrDefaultAsync();

                Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());
                Assert.IsNotNull(product1);

                var any = await context.Products.OrderBy(product => product.ProductNumber)
                    .Where(product => product.IsActive == isActive && product.ProductName == "Product2")
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .AnyAsync();

                Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());
                Assert.IsTrue(any);

                var sum = await context.Products.OrderBy(product => product.ProductNumber)
                    .Where(product => product.IsActive == isActive && product.ProductName == "Product2")
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .SumAsync(x => x.ProductId);

                Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());
                Assert.IsGreaterThan(lowerBound: 0, sum);
            });

    [TestMethod]
    [DataRow(TestCacheProvider.BuiltInInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
    [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
    [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
    public async Task TestSecondLevelCacheUsingTwoCountAsyncMethods(TestCacheProvider cacheProvider)
        => await EFServiceProvider.RunInContextAsync(cacheProvider, LogLevel.Debug, cacheAllQueries: false,
            async (context, loggerProvider) =>
            {
                var isActive = true;
                var name = "Product2";

                var count = await context.Products.OrderBy(product => product.ProductNumber)
                    .Where(product => product.IsActive == isActive && product.ProductName == name)
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .CountAsync();

                Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());
                Assert.IsGreaterThan(lowerBound: 0, count);

                count = await context.Products.OrderBy(product => product.ProductNumber)
                    .Where(product => product.IsActive == isActive && product.ProductName == name)
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .CountAsync();

                Assert.AreEqual(expected: 1, loggerProvider.GetCacheHitCount());
                Assert.IsGreaterThan(lowerBound: 0, count);
            });
}