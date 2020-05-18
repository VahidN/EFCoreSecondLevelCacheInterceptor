using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EFCoreSecondLevelCacheInterceptor.Tests
{
    [TestClass]
    public class EFCachePolicyParserTests
    {
        [TestMethod]
        public void TestGetEFCachePolicyWith2Parts()
        {
            const string commandText = @"-- EFCachePolicy[Index(27)] --> Absolute|00:45:00

      SELECT TOP(1) [p].[Id], [p].[Title], [p].[UserId], [p].[post_type], [u].[Id], [u].[Name], [u].[UserStatus]
      FROM [Posts] AS [p]
      INNER JOIN [Users] AS [u] ON [p].[UserId] = [u].[Id]
      WHERE [p].[post_type] IN (N'post_base', N'post_page') AND ([p].[Id] > @__param1_0)
      ORDER BY [p].[Id]";

            var cachePolicyParser = EFServiceProvider.GetRequiredService<IEFCachePolicyParser>();
            var cachePolicy = cachePolicyParser.GetEFCachePolicy(commandText);

            Assert.AreEqual(expected: CacheExpirationMode.Absolute, actual: cachePolicy.CacheExpirationMode);
            Assert.AreEqual(expected: TimeSpan.FromMinutes(45), actual: cachePolicy.CacheTimeout);
        }

        [TestMethod]
        public void TestGetEFCachePolicyWithAdditionalTagComments()
        {
            const string commandText =
@"-- CustomTagAbove

-- EFCachePolicy[Index(27)] --> Absolute|00:45:00

-- CustomTagBelow

SELECT TOP(1) [p].[Id], [p].[Title], [p].[UserId], [p].[post_type], [u].[Id], [u].[Name], [u].[UserStatus]
FROM [Posts] AS [p]
INNER JOIN [Users] AS [u] ON [p].[UserId] = [u].[Id]
WHERE [p].[post_type] IN (N'post_base', N'post_page') AND ([p].[Id] > @__param1_0)
ORDER BY [p].[Id]";

            var cachePolicyParser = EFServiceProvider.GetRequiredService<IEFCachePolicyParser>();
            var cachePolicy = cachePolicyParser.GetEFCachePolicy(commandText);

            Assert.AreEqual(expected: CacheExpirationMode.Absolute, actual: cachePolicy.CacheExpirationMode);
            Assert.AreEqual(expected: TimeSpan.FromMinutes(45), actual: cachePolicy.CacheTimeout);
        }

        [TestMethod]
        public void TestGetEFCachePolicyWithAllParts()
        {
            string commandText = "-- " + EFCachePolicy.Configure(options =>
                    options.ExpirationMode(CacheExpirationMode.Absolute)
                    .Timeout(TimeSpan.FromMinutes(45))
                    .SaltKey("saltKey")
                    .CacheDependencies("item 1", "item 2"));

            var cachePolicyParser = EFServiceProvider.GetRequiredService<IEFCachePolicyParser>();
            var cachePolicy = cachePolicyParser.GetEFCachePolicy(commandText);

            Assert.IsNotNull(cachePolicy);
            Assert.AreEqual(expected: CacheExpirationMode.Absolute, actual: cachePolicy.CacheExpirationMode);
            Assert.AreEqual(expected: TimeSpan.FromMinutes(45), actual: cachePolicy.CacheTimeout);
            Assert.AreEqual(expected: "saltKey", actual: cachePolicy.CacheSaltKey);
            CollectionAssert.AreEqual(expected: new SortedSet<string> { "item 1", "item 2" }, actual: cachePolicy.CacheItemsDependencies as SortedSet<string>);
        }

        [TestMethod]
        public void TestRemoveEFCachePolicyTagWithAdditionalTagComments()
        {
            const string commandText =
@"-- CustomTagAbove

-- EFCachePolicy[Index(27)] --> Absolute|00:45:00

-- CustomTagBelow

SELECT TOP(1) [p].[Id], [p].[Title], [p].[UserId], [p].[post_type], [u].[Id], [u].[Name], [u].[UserStatus]
FROM [Posts] AS [p]
INNER JOIN [Users] AS [u] ON [p].[UserId] = [u].[Id]
WHERE [p].[post_type] IN (N'post_base', N'post_page') AND ([p].[Id] > @__param1_0)
ORDER BY [p].[Id]";

            var cachePolicyParser = EFServiceProvider.GetRequiredService<IEFCachePolicyParser>();
            var commandTextWithCachePolicyTagRemoved = cachePolicyParser.RemoveEFCachePolicyTag(commandText);

            var expectedResult =
@"-- CustomTagAbove

-- CustomTagBelow

SELECT TOP(1) [p].[Id], [p].[Title], [p].[UserId], [p].[post_type], [u].[Id], [u].[Name], [u].[UserStatus]
FROM [Posts] AS [p]
INNER JOIN [Users] AS [u] ON [p].[UserId] = [u].[Id]
WHERE [p].[post_type] IN (N'post_base', N'post_page') AND ([p].[Id] > @__param1_0)
ORDER BY [p].[Id]";

            Assert.AreEqual(expected: expectedResult, actual: commandTextWithCachePolicyTagRemoved);
        }
    }
}