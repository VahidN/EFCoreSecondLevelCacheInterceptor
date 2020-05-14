using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EFCoreSecondLevelCacheInterceptor.Tests
{
    [TestClass]
    public class SecondLevelCacheInterceptorRemoveCachePolicyCommentTests
    {
        [DataTestMethod]
        [DataRow(TestCacheProvider.BuiltInInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
        [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
        [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
        public void SecondLevelCache_with_remove_comment_option_enabled_doesnt_hit_the_database(TestCacheProvider cacheProvider)
        {

            Action<EFCoreSecondLevelCacheOptions> configureCacheOptions = (options) => options.RemoveCachePolicyTagBeforeExecution(true);

            var testDbCommandInterceptor = new TestDbCommandInterceptor();
            Action<DbContextOptionsBuilder> configureDbContextOptions = (optionsBuilder) => optionsBuilder.AddInterceptors(testDbCommandInterceptor);

            EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, false, null, configureCacheOptions, configureDbContextOptions, (context, loggerProvider) =>
            {
                var isActive = true;
                var productName = "Product2";

                var list1 = context.Products
                    .TagWith("Custom Tag")
                    .OrderBy(product => product.ProductNumber)
                    .Where(product => product.IsActive == isActive && product.ProductName == productName)
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                    .ToList();

                Assert.IsTrue(list1.Any());
                Assert.AreEqual(0, loggerProvider.GetCacheHitCount());

                var actualSqlExecuted1 = testDbCommandInterceptor.ExecutedCommandText[0];
                var expectedSql1 =
@"-- Custom Tag

SELECT [p].[ProductId], [p].[IsActive], [p].[Notes], [p].[ProductName], [p].[ProductNumber], [p].[UserId]
FROM [Products] AS [p]
WHERE ([p].[IsActive] = @__isActive_0) AND ([p].[ProductName] = @__productName_1)
ORDER BY [p].[ProductNumber]";

                Assert.AreEqual(actualSqlExecuted1, expectedSql1);

                var list2 = context.Products
                    .TagWith("Custom Tag")
                    .OrderBy(product => product.ProductNumber)
                    .Where(product => product.IsActive == isActive && product.ProductName == productName)
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                    .ToList();

                Assert.IsTrue(list2.Any());
                Assert.AreEqual(1, loggerProvider.GetCacheHitCount());

                var actualSqlExecuted2 = testDbCommandInterceptor.ExecutedCommandText[1];
                var expectedSql2 =
@"-- Custom Tag

SELECT [p].[ProductId], [p].[IsActive], [p].[Notes], [p].[ProductName], [p].[ProductNumber], [p].[UserId]
FROM [Products] AS [p]
WHERE ([p].[IsActive] = @__isActive_0) AND ([p].[ProductName] = @__productName_1)
ORDER BY [p].[ProductNumber]";

                Assert.AreEqual(actualSqlExecuted2, expectedSql2);
            });
        }

        [DataTestMethod]
        [DataRow(TestCacheProvider.BuiltInInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
        [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
        [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
        public async Task SecondLevelCache_with_remove_comment_option_enabled_doesnt_hit_the_database_async(TestCacheProvider cacheProvider)
        {
            Action<EFCoreSecondLevelCacheOptions> configureCacheOptions = (options) => options.RemoveCachePolicyTagBeforeExecution(true);

            var testDbCommandInterceptor = new TestDbCommandInterceptor();
            Action<DbContextOptionsBuilder> configureDbContextOptions = (optionsBuilder) => optionsBuilder.AddInterceptors(testDbCommandInterceptor);

            await EFServiceProvider.RunInContextAsync(cacheProvider, LogLevel.Debug, false, null, configureCacheOptions, configureDbContextOptions, 
                async (context, loggerProvider) =>
                {
                    var isActive = true;
                    var productName = "Product2";

                    var list1 = await context.Products
                        .TagWith("Custom Tag")
                        .OrderBy(product => product.ProductNumber)
                        .Where(product => product.IsActive == isActive && product.ProductName == productName)
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .ToListAsync();

                    Assert.IsTrue(list1.Any());
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());

                    var actualSqlExecuted1 = testDbCommandInterceptor.ExecutedCommandText[0];
                    var expectedSql1 =
@"-- Custom Tag

SELECT [p].[ProductId], [p].[IsActive], [p].[Notes], [p].[ProductName], [p].[ProductNumber], [p].[UserId]
FROM [Products] AS [p]
WHERE ([p].[IsActive] = @__isActive_0) AND ([p].[ProductName] = @__productName_1)
ORDER BY [p].[ProductNumber]";

                    Assert.AreEqual(actualSqlExecuted1, expectedSql1);

                    var list2 = await context.Products
                        .TagWith("Custom Tag")
                        .OrderBy(product => product.ProductNumber)
                        .Where(product => product.IsActive == isActive && product.ProductName == productName)
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .ToListAsync();

                    Assert.IsTrue(list2.Any());
                    Assert.AreEqual(1, loggerProvider.GetCacheHitCount());

                    var actualSqlExecuted2 = testDbCommandInterceptor.ExecutedCommandText[1];
                    var expectedSql2 =
@"-- Custom Tag

SELECT [p].[ProductId], [p].[IsActive], [p].[Notes], [p].[ProductName], [p].[ProductNumber], [p].[UserId]
FROM [Products] AS [p]
WHERE ([p].[IsActive] = @__isActive_0) AND ([p].[ProductName] = @__productName_1)
ORDER BY [p].[ProductNumber]";

                    Assert.AreEqual(actualSqlExecuted2, expectedSql2);
                })
                .ConfigureAwait(false);
        }

        private class TestDbCommandInterceptor : DbCommandInterceptor
        {
            public List<string> ExecutedCommandText { get; } = new List<string>();

            public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
            {
                ExecutedCommandText.Add(command.CommandText);

                return base.ReaderExecuted(command, eventData, result);
            }

            public override Task<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken = default)
            {
                ExecutedCommandText.Add(command.CommandText);

                return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
            }
        }
    }
}
