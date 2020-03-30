using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EFCoreSecondLevelCacheInterceptor.Tests
{
    [TestClass]
    public class EFCacheDependenciesProcessorTests
    {
        [TestMethod]
        public void TestGetCacheDependenciesWorks()
        {
            const string commandText = @"-- EFCachePolicy[Index(27)] --> Absolute|00:45:00

      SELECT TOP(1) [p].[Id], [p].[Title], [p].[UserId], [p].[post_type], [u].[Id], [u].[Name], [u].[UserStatus]
      FROM [Posts] AS [p]
      INNER JOIN [Users] AS [u] ON [p].[UserId] = [u].[Id]
      WHERE [p].[post_type] IN (N'post_base', N'post_page') AND ([p].[Id] > @__param1_0)
      ORDER BY [p].[Id]";
            var cacheDependenciesProcessor = EFServiceProvider.GetRequiredService<IEFCacheDependenciesProcessor>();
            var cacheDependencies = cacheDependenciesProcessor.GetCacheDependencies(new EFCachePolicy(), new SortedSet<string> { "Posts", "Users", "Products" }, commandText);

            var inUseTableNames = new SortedSet<string> { "Posts", "Users" };
            CollectionAssert.AreEqual(expected: inUseTableNames, actual: cacheDependencies);
        }

        [TestMethod]
        public void TestGetCacheDependenciesWorksWithASquareBracketInsideAStringValue()
        {
            const string commandText = @"-- EFCachePolicy[Index(27)] --> Absolute|00:45:00

      SELECT TOP(1) [p].[Id], [p].[Title], [p].[UserId], [p].[post_type], [u].[Id], [u].[Name], [u].[UserStatus]
      FROM [Posts] AS [p]
      INNER JOIN [Users] AS [u] ON [p].[UserId] = [u].[Id]
      WHERE [p].[post_type] IN (N'post_base', N'post_page') AND ([p].[Id] > @__param1_0) and [u].[Name]=' [Products] '
      ORDER BY [p].[Id]";
            var cacheDependenciesProcessor = EFServiceProvider.GetRequiredService<IEFCacheDependenciesProcessor>();
            var cacheDependencies = cacheDependenciesProcessor.GetCacheDependencies(new EFCachePolicy(), new SortedSet<string> { "Posts", "Users", "Products" }, commandText);

            var inUseTableNames = new SortedSet<string> { "Posts", "Users" };
            CollectionAssert.AreEqual(expected: inUseTableNames, actual: cacheDependencies);
        }

        [TestMethod]
        public void TestGetCacheDependenciesWorksWithAQuoteInsideAStringValue()
        {
            const string commandText = @"-- EFCachePolicy[Index(27)] --> Absolute|00:45:00

      SELECT TOP(1) [p].[Id], [p].[Title], [p].[UserId], [p].[post_type], [u].[Id], [u].[Name], [u].[UserStatus]
      FROM [Posts] AS [p]
      INNER JOIN [Users] AS [u] ON [p].[UserId] = [u].[Id]
      WHERE [p].[post_type] IN (N'post_base', N'post_page') AND ([p].[Id] > @__param1_0) and [u].[Name]=' ""Products"" '
      ORDER BY [p].[Id]";
            var cacheDependenciesProcessor = EFServiceProvider.GetRequiredService<IEFCacheDependenciesProcessor>();
            var cacheDependencies = cacheDependenciesProcessor.GetCacheDependencies(new EFCachePolicy(), new SortedSet<string> { "Posts", "Users", "Products" }, commandText);

            var inUseTableNames = new SortedSet<string> { "Posts", "Users" };
            CollectionAssert.AreEqual(expected: inUseTableNames, actual: cacheDependencies);
        }

        [TestMethod]
        public void TestGetCacheDependenciesWorksForInserts()
        {
            const string commandText = @"SET NOCOUNT ON;
        INSERT INTO [Products] ([IsActive], [Notes], [ProductName], [ProductNumber], [UserId])
        VALUES (@p0, @p1, @p2, @p3, @p4);
        SELECT [ProductId]
        FROM [Products]
        WHERE @@ROWCOUNT = 1 AND [ProductId] = scope_identity();";
            var cacheDependenciesProcessor = EFServiceProvider.GetRequiredService<IEFCacheDependenciesProcessor>();
            var cacheDependencies = cacheDependenciesProcessor.GetCacheDependencies(new EFCachePolicy(), new SortedSet<string> { "Posts", "Users", "Products" }, commandText);

            var inUseTableNames = new SortedSet<string> { "Products" };
            CollectionAssert.AreEqual(expected: inUseTableNames, actual: cacheDependencies);
        }

        [TestMethod]
        public void TestGetCacheDependenciesWorksForInsertsWithBacktick()
        {
            const string commandText = @"SET NOCOUNT ON;
        INSERT INTO `Products` (`IsActive`, `Notes`, `ProductName`, `ProductNumber`, `UserId`)
        VALUES (@p0, @p1, @p2, @p3, @p4);
        SELECT `ProductId`
        FROM `Products`
        WHERE @@ROWCOUNT = 1 AND `ProductId` = scope_identity();";
            var cacheDependenciesProcessor = EFServiceProvider.GetRequiredService<IEFCacheDependenciesProcessor>();
            var cacheDependencies = cacheDependenciesProcessor.GetCacheDependencies(new EFCachePolicy(), new SortedSet<string> { "Posts", "Users", "Products" }, commandText);

            var inUseTableNames = new SortedSet<string> { "Products" };
            CollectionAssert.AreEqual(expected: inUseTableNames, actual: cacheDependencies);
        }

        [TestMethod]
        public void TestGetCacheDependenciesWorksForDeletes()
        {
            const string commandText = @"SET NOCOUNT ON;
        DELETE FROM [Products]
        WHERE [ProductId] = @p0;
        SELECT @@ROWCOUNT;";
            var cacheDependenciesProcessor = EFServiceProvider.GetRequiredService<IEFCacheDependenciesProcessor>();
            var cacheDependencies = cacheDependenciesProcessor.GetCacheDependencies(new EFCachePolicy(), new SortedSet<string> { "Posts", "Users", "Products" }, commandText);

            var inUseTableNames = new SortedSet<string> { "Products" };
            CollectionAssert.AreEqual(expected: inUseTableNames, actual: cacheDependencies);
        }

        [TestMethod]
        public void TestGetCacheDependenciesWorksForUpdates()
        {
            const string commandText = @"SET NOCOUNT ON;
      UPDATE [Users] SET [UserStatus] = @p0
      WHERE [Id] = @p1;
      SELECT @@ROWCOUNT;";
            var cacheDependenciesProcessor = EFServiceProvider.GetRequiredService<IEFCacheDependenciesProcessor>();
            var cacheDependencies = cacheDependenciesProcessor.GetCacheDependencies(new EFCachePolicy(), new SortedSet<string> { "Posts", "Users", "Products" }, commandText);

            var inUseTableNames = new SortedSet<string> { "Users" };
            CollectionAssert.AreEqual(expected: inUseTableNames, actual: cacheDependencies);
        }
    }
}