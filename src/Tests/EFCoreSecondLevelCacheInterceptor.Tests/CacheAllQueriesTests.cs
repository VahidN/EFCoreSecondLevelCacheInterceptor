using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EFCoreSecondLevelCacheInterceptor.Tests
{
    [TestClass]
    public class CacheAllQueriesTests  
    {
        [DataTestMethod]
        [DataRow(TestCacheProvider.BuiltInInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
        [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
        [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
        public async Task TestCacheAllQueriesWorks(TestCacheProvider cacheProvider)
        {
            await EFServiceProvider.RunInContextAsync(cacheProvider, LogLevel.Debug, true,
                async (context, loggerProvider) =>
                {
                    var isActive = true;
                    var name = "Product1";

                    var list1 = await context.Products
                        .OrderBy(product => product.ProductNumber)
                        .Where(product => product.IsActive == isActive && product.ProductName == name)
                        .ToListAsync();
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                    Assert.IsTrue(list1.Any());

                    var list2 = await context.Products
                        .OrderBy(product => product.ProductNumber)
                        .Where(product => product.IsActive == isActive && product.ProductName == name)
                        .ToListAsync();
                    Assert.AreEqual(1, loggerProvider.GetCacheHitCount());
                    Assert.IsTrue(list2.Any());
                });
        }

        [DataTestMethod]
        [DataRow(TestCacheProvider.BuiltInInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
        [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
        [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
        public async Task TestCacheAllQueriesWithNotCacheableWorks(TestCacheProvider cacheProvider)
        {
            await EFServiceProvider.RunInContextAsync(cacheProvider, LogLevel.Debug, true,
                async (context, loggerProvider) =>
                {
                    var isActive = true;
                    var name = "Product1";

                    var list1 = await context.Products
                        .OrderBy(product => product.ProductNumber)
                        .Where(product => product.IsActive == isActive && product.ProductName == name)
                        .NotCacheable()
                        .ToListAsync();
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                    Assert.IsTrue(list1.Any());

                    var list2 = await context.Products
                        .OrderBy(product => product.ProductNumber)
                        .Where(product => product.IsActive == isActive && product.ProductName == name)
                        .NotCacheable()
                        .ToListAsync();
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                    Assert.IsTrue(list2.Any());
                });
        }
    }
}