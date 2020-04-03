using System.Linq;
using System.Threading.Tasks;
using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EFCoreSecondLevelCacheInterceptor.Tests
{
    [TestClass]
    public class SecondLevelCacheInterceptorSpTests
    {
        [DataTestMethod]
        [DataRow(TestCacheProvider.BuiltInInMemory)]
        [DataRow(TestCacheProvider.BuiltInRedis)]
        [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
        public async Task TestCacheableWorksWithSPs(TestCacheProvider cacheProvider)
        {
            await EFServiceProvider.RunInContextAsync(cacheProvider, LogLevel.Debug, false, async (context, loggerProvider) =>
                   {
                       var blogId = 1;
                       var blogs = await context.Set<BlogData>()
                                               .FromSqlRaw("usp_GetBlogData {0}", blogId)
                                               .Cacheable()
                                               .ToListAsync();
                       Assert.IsTrue(blogs.Any());
                       Assert.AreEqual(0, loggerProvider.GetCacheHitCount());

                       blogs = await context.Set<BlogData>()
                                               .FromSqlRaw("usp_GetBlogData {0}", blogId)
                                               .Cacheable()
                                               .ToListAsync();
                       Assert.IsTrue(blogs.Any());
                       Assert.AreEqual(1, loggerProvider.GetCacheHitCount());
                   });
        }

        [DataTestMethod]
        [DataRow(TestCacheProvider.BuiltInInMemory)]
        [DataRow(TestCacheProvider.BuiltInRedis)]
        [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
        public async Task TestCacheInvalidationWorksWithSPs(TestCacheProvider cacheProvider)
        {
            await EFServiceProvider.RunInContextAsync(cacheProvider, LogLevel.Debug, false, async (context, loggerProvider) =>
                   {
                       var blogId = 1;
                       var blogs = await context.Set<BlogData>()
                                               .FromSqlRaw("usp_GetBlogData {0}", blogId)
                                               .Cacheable()
                                               .ToListAsync();
                       Assert.IsTrue(blogs.Any());
                       Assert.AreEqual(0, loggerProvider.GetCacheHitCount());

                       blogs = await context.Set<BlogData>()
                                               .FromSqlRaw("usp_GetBlogData {0}", blogId)
                                               .Cacheable()
                                               .ToListAsync();
                       Assert.IsTrue(blogs.Any());
                       Assert.AreEqual(1, loggerProvider.GetCacheHitCount());

                       var product = new Product
                       {
                           IsActive = false,
                           ProductName = $"Product{RandomNumberProvider.Next()}",
                           ProductNumber = RandomNumberProvider.Next().ToString(),
                           Notes = "Notes ...",
                           UserId = 1
                       };
                       context.Products.Add(product);
                       await context.SaveChangesAsync();

                       blogs = await context.Set<BlogData>()
                                               .FromSqlRaw("usp_GetBlogData {0}", blogId)
                                               .Cacheable()
                                               .ToListAsync();
                       Assert.IsTrue(blogs.Any());
                       Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                   });
        }
    }
}