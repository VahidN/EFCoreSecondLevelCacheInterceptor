using Microsoft.Extensions.Logging;

namespace EFCoreSecondLevelCacheInterceptor.Tests;

[TestClass]
public class SecondLevelCacheInterceptorTransactionTests
{
    [TestMethod]
    [DataRow(TestCacheProvider.BuiltInInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
    [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
    [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
    public void TestQueriesUsingExplicitTransactionsWillNotUseTheCache(TestCacheProvider cacheProvider)
        => EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, cacheAllQueries: false,
            (context, loggerProvider) =>
            {
                Assert.ThrowsExactly<TimeoutException>(() =>
                {
                    using (var txn = context.Database.BeginTransaction())
                    {
                        // Read and modify an entity
                        var entity1 = context.Tags
                            .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                            .First();

                        entity1.Name = "FOO";

                        // Save the change, cache will be invalidated
                        context.SaveChanges();

                        // Read the same entity again
                        entity1 = context.Tags
                            .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                            .First();

                        // It will not get cached
                        Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());

                        // Call some method
                        // THIS METHOD THROWS AN EXCEPTION SO THE TRANSACTION IS NEVER COMMITTED
                        throw new TimeoutException();

                        // (we never hit these lines, so the cache is not invalidated and the transaction is not committed)
#pragma warning disable CS0162 // Unreachable code detected
                        context.SaveChanges();
                        txn.Commit();
#pragma warning restore CS0162 // Unreachable code detected
                    }
                });
            });

    [TestMethod]
    [DataRow(TestCacheProvider.BuiltInInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
    [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
    [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
    public void TestQueriesUsingExplicitTransactionsWillInvalidateTheCache(TestCacheProvider cacheProvider)
    {
        var rnd = new Random();

        EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, cacheAllQueries: false, (context, loggerProvider)
            =>
        {
            // Read and cache data
            var entity0 = context.Tags.Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                .First();

            using (var txn = context.Database.BeginTransaction())
            {
                // Read and modify an entity.
                var entity1 = context.Tags.Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .First();

                // Reading the data from the database, not cache.
                Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());
                entity1.Name = $"FOO{rnd.Next()}";

                // Save the change, cache will be invalidated.
                context.SaveChanges();

                // Read the same entity again.
                entity1 = context.Tags.Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .First();

                // Reading the data from the database, not cache.
                Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());

                context.SaveChanges();
                txn.Commit();
            }

            // `After` committing the transaction, the related query cache should be invalidated.

            var entity2 = context.Tags.Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                .First();

            // Reading the data from the database after invalidation, not cache.
            Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());
        });
    }
}