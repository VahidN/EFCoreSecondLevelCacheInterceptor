using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EFCoreSecondLevelCacheInterceptor.Tests
{
    [TestClass]
    public class SecondLevelCacheInterceptorTransactionTests
    {
        [DataTestMethod]
        [DataRow(TestCacheProvider.BuiltInInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
        [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
        [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
        [ExpectedException(typeof(TimeoutException))]
        public void TestQueriesUsingExplicitTransactionsWillNotUseTheCache(TestCacheProvider cacheProvider)
        {
            EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, false, (context, loggerProvider) =>
            {
                using (var txn = context.Database.BeginTransaction())
                {
                    // Read and modify an entity
                    var entity1 = context.Tags.Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45)).First();
                    entity1.Name = "FOO";

                    // Save the change, cache will be invalidated
                    context.SaveChanges();

                    // Read the same entity again
                    entity1 = context.Tags.Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45)).First();
                    // It will not get cached
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());

                    // Call some method
                    // THIS METHOD THROWS AN EXCEPTION SO THE TRANSACTION IS NEVER COMMITTED
                    throw new TimeoutException();

                    // (we never hit these lines, so the cache is not invalidated and the transaction is not committed)
                    context.SaveChanges();
                    txn.Commit();
                }
            });
        }

        [DataTestMethod]
        [DataRow(TestCacheProvider.BuiltInInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
        [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
        [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
        public void TestQueriesUsingExplicitTransactionsWillInvalidateTheCache(TestCacheProvider cacheProvider)
        {
            var rnd = new Random();

            EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, false, (context, loggerProvider) =>
            {
                // Read and cache data
                var entity0 = context.Tags.Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45)).First();

                using (var txn = context.Database.BeginTransaction())
                {
                    // Read and modify an entity.
                    var entity1 = context.Tags.Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45)).First();
                    // Reading the data from the database, not cache.
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                    entity1.Name = $"FOO{rnd.Next()}";

                    // Save the change, cache will be invalidated.
                    context.SaveChanges();

                    // Read the same entity again.
                    entity1 = context.Tags.Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45)).First();
                    // Reading the data from the database, not cache.
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());

                    context.SaveChanges();
                    txn.Commit();
                }

                // `After` committing the transaction, the related query cache should be invalidated.

                var entity2 = context.Tags.Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45)).First();
                // Reading the data from the database after invalidation, not cache.
                Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
            });
        }
    }
}