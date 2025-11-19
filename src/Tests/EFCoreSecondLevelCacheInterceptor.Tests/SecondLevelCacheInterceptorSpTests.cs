using System.Globalization;
using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EFCoreSecondLevelCacheInterceptor.Tests;

[TestClass]
public class SecondLevelCacheInterceptorSpTests
{
    [TestMethod]
    [DataRow(TestCacheProvider.BuiltInInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
    [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
    [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
    public async Task TestCacheableWorksWithSPs(TestCacheProvider cacheProvider)
        => await EFServiceProvider.RunInContextAsync(cacheProvider, LogLevel.Debug, cacheAllQueries: false,
            async (context, loggerProvider) =>
            {
                var blogId = 1;

                var blogs = await context.Set<BlogData>()
                    .FromSqlRaw(sql: "usp_GetBlogData {0}", blogId)
                    .Cacheable()
                    .ToListAsync();

                Assert.IsTrue(blogs.Any());
                Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());

                blogs = await context.Set<BlogData>()
                    .FromSqlRaw(sql: "usp_GetBlogData {0}", blogId)
                    .Cacheable()
                    .ToListAsync();

                Assert.IsTrue(blogs.Any());
                Assert.AreEqual(expected: 1, loggerProvider.GetCacheHitCount());
            });

    [TestMethod]
    [DataRow(TestCacheProvider.BuiltInInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
    [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
    [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
    public async Task TestCacheInvalidationWorksWithSPs(TestCacheProvider cacheProvider)
        => await EFServiceProvider.RunInContextAsync(cacheProvider, LogLevel.Debug, cacheAllQueries: false,
            async (context, loggerProvider) =>
            {
                var blogId = 1;

                var blogs = await context.Set<BlogData>()
                    .FromSqlRaw(sql: "usp_GetBlogData {0}", blogId)
                    .Cacheable()
                    .ToListAsync();

                Assert.IsTrue(blogs.Any());
                Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());

                blogs = await context.Set<BlogData>()
                    .FromSqlRaw(sql: "usp_GetBlogData {0}", blogId)
                    .Cacheable()
                    .ToListAsync();

                Assert.IsTrue(blogs.Any());
                Assert.AreEqual(expected: 1, loggerProvider.GetCacheHitCount());

                var product = new Product
                {
                    IsActive = false,
                    ProductName = $"Product{RandomNumberProvider.Next()}",
                    ProductNumber = RandomNumberProvider.Next().ToString(CultureInfo.InvariantCulture),
                    Notes = "Notes ...",
                    UserId = 1
                };

                context.Products.Add(product);
                await context.SaveChangesAsync();

                blogs = await context.Set<BlogData>()
                    .FromSqlRaw(sql: "usp_GetBlogData {0}", blogId)
                    .Cacheable()
                    .ToListAsync();

                Assert.IsTrue(blogs.Any());
                Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());
            });
}