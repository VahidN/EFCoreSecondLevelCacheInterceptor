using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EFCoreSecondLevelCacheInterceptor.Tests
{
    [TestClass]
    public class SecondLevelCacheInterceptorAsyncTests
    {
        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public async Task TestSecondLevelCacheUsingAsyncMethodsDoesNotHitTheDatabase(bool useRedis)
        {
            await EFServiceProvider.RunInContextAsync(useRedis, LogLevel.Information, false,
                async (context, loggerProvider) =>
                {
                    var isActive = true;
                    var name = "Product1";

                    var list1 = await context.Products
                        .OrderBy(product => product.ProductNumber)
                        .Where(product => product.IsActive == isActive && product.ProductName == name)
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .ToListAsync();
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                    Assert.IsTrue(list1.Any());

                    var list2 = await context.Products
                        .OrderBy(product => product.ProductNumber)
                        .Where(product => product.IsActive == isActive && product.ProductName == name)
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .ToListAsync();
                    Assert.AreEqual(1, loggerProvider.GetCacheHitCount());
                    Assert.IsTrue(list2.Any());

                    var list3 = await context.Products
                        .OrderBy(product => product.ProductNumber)
                        .Where(product => product.IsActive == isActive && product.ProductName == name)
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .ToListAsync();
                    Assert.AreEqual(1, loggerProvider.GetCacheHitCount());
                    Assert.IsTrue(list3.Any());

                    var list4 = await context.Products
                        .OrderBy(product => product.ProductNumber)
                        .Where(product => product.IsActive == isActive && product.ProductName == "Product2")
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .ToListAsync();
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                    Assert.IsTrue(list4.Any());

                    var product1 = await context.Products
                        .OrderBy(product => product.ProductNumber)
                        .Where(product => product.IsActive == isActive && product.ProductName == "Product2")
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .FirstOrDefaultAsync();
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                    Assert.IsNotNull(product1);
                });
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public async Task TestSecondLevelCacheUsingDifferentAsyncMethods(bool useRedis)
        {
            await EFServiceProvider.RunInContextAsync(useRedis, LogLevel.Information, false,
                async (context, loggerProvider) =>
                {
                    var isActive = true;
                    var name = "Product3";

                    var list1 = await context.Products
                        .OrderBy(product => product.ProductNumber)
                        .Where(product => product.IsActive == isActive && product.ProductName == name)
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .ToListAsync();
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                    Assert.IsTrue(list1.Any());

                    var count = await context.Products
                        .OrderBy(product => product.ProductNumber)
                        .Where(product => product.IsActive == isActive && product.ProductName == name)
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .CountAsync();
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                    Assert.IsTrue(count > 0);

                    var product1 = await context.Products
                        .OrderBy(product => product.ProductNumber)
                        .Where(product => product.IsActive == isActive && product.ProductName == name)
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .FirstOrDefaultAsync();
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                    Assert.IsTrue(product1 != null);

                    var any = await context.Products
                        .OrderBy(product => product.ProductNumber)
                        .Where(product => product.IsActive == isActive && product.ProductName == "Product2")
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .AnyAsync();
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                    Assert.IsTrue(any);

                    var sum = await context.Products
                        .OrderBy(product => product.ProductNumber)
                        .Where(product => product.IsActive == isActive && product.ProductName == "Product2")
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .SumAsync(x => x.ProductId);
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                    Assert.IsTrue(sum > 0);
                });
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public async Task TestSecondLevelCacheUsingTwoCountAsyncMethods(bool useRedis)
        {
            await EFServiceProvider.RunInContextAsync(useRedis, LogLevel.Information, false,
                async (context, loggerProvider) =>
                {
                    var isActive = true;
                    var name = "Product2";

                    var count = await context.Products
                        .OrderBy(product => product.ProductNumber)
                        .Where(product => product.IsActive == isActive && product.ProductName == name)
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .CountAsync();
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                    Assert.IsTrue(count > 0);

                    count = await context.Products
                        .OrderBy(product => product.ProductNumber)
                        .Where(product => product.IsActive == isActive && product.ProductName == name)
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .CountAsync();
                    Assert.AreEqual(1, loggerProvider.GetCacheHitCount());
                    Assert.IsTrue(count > 0);
                });
        }

        /*[DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public async Task TestSecondLevelCacheUsingFindAsyncMethods(bool useRedis)
        {
            await EFServiceProvider.RunInContextAsync(useRedis, LogLevel.Information, false,
                async (context, loggerProvider) =>
                {
                    var product1 = context.Products.TagWith("query 1").Find(1);

                    var product1 = await context.Products
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .FindAsync(1);
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                    Assert.IsTrue(product1 != null);

                    product1 = await context.Products
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .FindAsync(1);
                    Assert.AreEqual(1, loggerProvider.GetCacheHitCount());
                    Assert.IsTrue(product1 != null);
                });
        }*/
    }
}