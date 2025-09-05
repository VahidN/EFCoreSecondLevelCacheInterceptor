using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Assert = Xunit.Assert;

namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

public class EFCacheDependenciesProcessorTests
{
    private const string CacheKeyPrefix = "EF_";
    private readonly IEFCacheDependenciesProcessor _efCacheDependenciesProcessor;

    public EFCacheDependenciesProcessorTests()
    {
        var cacheSettingsMock = new Mock<IOptions<EFCoreSecondLevelCacheSettings>>();

        cacheSettingsMock.Setup(c => c.Value)
            .Returns(new EFCoreSecondLevelCacheSettings
            {
                EnableLogging = false
            });

        var logger = new Mock<IEFDebugLogger>().Object;
        var cacheDependenciesProcessorLogger = new Mock<ILogger<EFCacheDependenciesProcessor>>().Object;
        var cacheServiceProvider = new Mock<IEFCacheServiceProvider>().Object;
        var serviceProvider = new Mock<IServiceProvider>().Object;
        var cacheKeyPrefixProviderMock = new EFCacheKeyPrefixProvider(serviceProvider, cacheSettingsMock.Object);
        var sqlCommandsProcessor = new EFSqlCommandsProcessor(new XxHash64Unsafe());

        _efCacheDependenciesProcessor = new EFCacheDependenciesProcessor(logger, cacheDependenciesProcessorLogger,
            cacheServiceProvider, sqlCommandsProcessor, cacheSettingsMock.Object, cacheKeyPrefixProviderMock);
    }

    [Fact]
    public void TestGetCacheDependenciesWorksWithNormalEFQueries()
    {
        // Arrange
        const string commandText = @"-- EFCachePolicy[TestCachingByteArrays(line 391)] --> Absolute|00:45:00|||False

      SELECT TOP(1) [u].[Id], [u].[AddDate], [u].[ByteArrayValue], [u].[ByteValue], [u].[CharValue], [u].[DateTimeOffsetValue], [u].[DecimalValue], [u].[DoubleValue], [u].[FloatValue], [u].[GuidValue], [u].[ImageData], [u].[IsActive], [u].[Name], [u].[Points], [u].[ShortValue], [u].[TimeSpanValue], [u].[UintValue], [u].[UlongValue], [u].[UpdateDate], [u].[UserStatus], [u].[UshortValue]
      FROM [Users] AS [u]
      WHERE [u].[Id] = @__user1_Id_0]";

        // Act        
        var cacheDependencies = _efCacheDependenciesProcessor.GetCacheDependencies(new EFCachePolicy(),
            ["Posts", "Users", "Products"], commandText);

        // Assert
        Assert.Equal([$"{CacheKeyPrefix}Users"], cacheDependencies);
    }

    [Fact]
    public void TestGetCacheDependenciesWorks()
    {
        const string commandText = @"-- EFCachePolicy[Index(27)] --> Absolute|00:45:00

      SELECT TOP(1) [p].[Id], [p].[Title], [p].[UserId], [p].[post_type], [u].[Id], [u].[Name], [u].[UserStatus]
      FROM [Posts] AS [p]
      INNER JOIN [Users] AS [u] ON [p].[UserId] = [u].[Id]
      WHERE [p].[post_type] IN (N'post_base', N'post_page') AND ([p].[Id] > @__param1_0)
      ORDER BY [p].[Id]";

        var cacheDependencies = _efCacheDependenciesProcessor.GetCacheDependencies(new EFCachePolicy(),
            ["Posts", "Users", "Products"], commandText);

        SortedSet<string> inUseTableNames = [$"{CacheKeyPrefix}Posts", $"{CacheKeyPrefix}Users"];

        Assert.Equal(inUseTableNames, cacheDependencies);
    }

    [Fact]
    public void TestGetCacheDependenciesWorksWithSchemas()
    {
        const string commandText = @"-- EFCachePolicy[Index(27)] --> Absolute|00:45:00

      SELECT TOP(1) [p].[Id], [p].[Title], [p].[UserId], [p].[post_type], [u].[Id], [u].[Name], [u].[UserStatus]
      FROM dbo.[Posts] AS [p]
      INNER JOIN [dbo].[Users] AS [u] ON [p].[UserId] = [u].[Id]
      WHERE [p].[post_type] IN (N'post_base', N'post_page') AND ([p].[Id] > @__param1_0)
      ORDER BY [p].[Id]";

        var cacheDependencies = _efCacheDependenciesProcessor.GetCacheDependencies(new EFCachePolicy(),
            ["Posts", "Users", "Products"], commandText);

        SortedSet<string> inUseTableNames = [$"{CacheKeyPrefix}Posts", $"{CacheKeyPrefix}Users"];

        Assert.Equal(inUseTableNames, cacheDependencies);
    }

    [Fact]
    public void TestGetCacheDependenciesWorksWithASquareBracketInsideAStringValue()
    {
        const string commandText = @"-- EFCachePolicy[Index(27)] --> Absolute|00:45:00

      SELECT TOP(1) [p].[Id], [p].[Title], [p].[UserId], [p].[post_type], [u].[Id], [u].[Name], [u].[UserStatus]
      FROM [Posts] AS [p]
      INNER JOIN [Users] AS [u] ON [p].[UserId] = [u].[Id]
      WHERE [p].[post_type] IN (N'post_base', N'post_page') AND ([p].[Id] > @__param1_0) and [u].[Name]=' [Products] '
      ORDER BY [p].[Id]";

        var cacheDependencies = _efCacheDependenciesProcessor.GetCacheDependencies(new EFCachePolicy(),
            ["Posts", "Users", "Products"], commandText);

        SortedSet<string> inUseTableNames = [$"{CacheKeyPrefix}Posts", $"{CacheKeyPrefix}Users"];

        Assert.Equal(inUseTableNames, cacheDependencies);
    }

    [Fact]
    public void TestGetCacheDependenciesWorksWithAQuoteInsideAStringValue()
    {
        const string commandText = @"-- EFCachePolicy[Index(27)] --> Absolute|00:45:00

      SELECT TOP(1) [p].[Id], [p].[Title], [p].[UserId], [p].[post_type], [u].[Id], [u].[Name], [u].[UserStatus]
      FROM [Posts] AS [p]
      INNER JOIN [Users] AS [u] ON [p].[UserId] = [u].[Id]
      WHERE [p].[post_type] IN (N'post_base', N'post_page') AND ([p].[Id] > @__param1_0) and [u].[Name]=' ""Products"" '
      ORDER BY [p].[Id]";

        var cacheDependencies = _efCacheDependenciesProcessor.GetCacheDependencies(new EFCachePolicy(),
            ["Posts", "Users", "Products"], commandText);

        SortedSet<string> inUseTableNames = [$"{CacheKeyPrefix}Posts", $"{CacheKeyPrefix}Users"];

        Assert.Equal(inUseTableNames, cacheDependencies);
    }

    [Fact]
    public void TestGetCacheDependenciesWorksForInserts()
    {
        const string commandText = @"SET NOCOUNT ON;
        INSERT INTO [Products] ([IsActive], [Notes], [ProductName], [ProductNumber], [UserId])
        VALUES (@p0, @p1, @p2, @p3, @p4);
        SELECT [ProductId]
        FROM [Products]
        WHERE @@ROWCOUNT = 1 AND [ProductId] = scope_identity();";

        var cacheDependencies = _efCacheDependenciesProcessor.GetCacheDependencies(new EFCachePolicy(),
            ["Posts", "Users", "Products"], commandText);

        SortedSet<string> inUseTableNames = [$"{CacheKeyPrefix}Products"];

        Assert.Equal(inUseTableNames, cacheDependencies);
    }

    [Fact]
    public void TestGetCacheDependenciesWorksForInsertsWithBacktick()
    {
        const string commandText = @"SET NOCOUNT ON;
        INSERT INTO `Products` (`IsActive`, `Notes`, `ProductName`, `ProductNumber`, `UserId`)
        VALUES (@p0, @p1, @p2, @p3, @p4);
        SELECT `ProductId`
        FROM `Products`
        WHERE @@ROWCOUNT = 1 AND `ProductId` = scope_identity();";

        var cacheDependencies = _efCacheDependenciesProcessor.GetCacheDependencies(new EFCachePolicy(),
            ["Posts", "Users", "Products"], commandText);

        SortedSet<string> inUseTableNames = [$"{CacheKeyPrefix}Products"];

        Assert.Equal(inUseTableNames, cacheDependencies);
    }

    [Fact]
    public void TestGetCacheDependenciesWorksForDeletes()
    {
        const string commandText = @"SET NOCOUNT ON;
        DELETE FROM [Products]
        WHERE [ProductId] = @p0;
        SELECT @@ROWCOUNT;";

        var cacheDependencies = _efCacheDependenciesProcessor.GetCacheDependencies(new EFCachePolicy(),
            ["Posts", "Users", "Products"], commandText);

        SortedSet<string> inUseTableNames = [$"{CacheKeyPrefix}Products"];

        Assert.Equal(inUseTableNames, cacheDependencies);
    }

    [Fact]
    public void TestGetCacheDependenciesWorksForUpdates()
    {
        const string commandText = @"SET NOCOUNT ON;
      UPDATE [Users] SET [UserStatus] = @p0
      WHERE [Id] = @p1;
      SELECT @@ROWCOUNT;";

        var cacheDependencies = _efCacheDependenciesProcessor.GetCacheDependencies(new EFCachePolicy(),
            ["Posts", "Users", "Products"], commandText);

        SortedSet<string> inUseTableNames = [$"{CacheKeyPrefix}Users"];

        Assert.Equal(inUseTableNames, cacheDependencies);
    }

    [Fact]
    public void TestGetCacheDependenciesWorksForBatchInserts()
    {
        const string commandText = @"SET NOCOUNT ON;
DECLARE @inserted2 TABLE ([BlogId] int, [_Position] [int]);
MERGE [Blogs] USING (
VALUES (@p4, @p5, 0),
(@p6, @p7, 1),
(@p8, @p9, 2),
(@p10, @p11, 3),
(@p12, @p13, 4),
(@p14, @p15, 5)) AS i ([Name], [Url], _Position) ON 1=0
WHEN NOT MATCHED THEN
INSERT ([Name], [Url])
VALUES (i.[Name], i.[Url])
OUTPUT INSERTED.[BlogId], i._Position
INTO @inserted2;";

        var cacheDependencies = _efCacheDependenciesProcessor.GetCacheDependencies(new EFCachePolicy(),
            ["Blogs", "Posts"], commandText);

        SortedSet<string> inUseTableNames = [$"{CacheKeyPrefix}Blogs"];

        Assert.Equal(inUseTableNames, cacheDependencies);
    }

    [Fact]
    public void TestGetCacheDependenciesWorksForBulkInsertOrUpdate()
    {
        const string commandText =
            @"MERGE [dbo].[People] WITH (HOLDLOCK) AS T USING (SELECT TOP 2 * FROM [dbo].[PeopleTemp94f5cba8] ORDER BY [Id]) AS S ON T.[Id] = S.[Id] WHEN NOT MATCHED BY TARGET THEN INSERT ([Name]) VALUES (S.[Name]) WHEN MATCHED AND EXISTS (SELECT S.[Name] EXCEPT SELECT T.[Name]) THEN UPDATE SET T.[Name] = S.[Name];";

        var cacheDependencies =
            _efCacheDependenciesProcessor.GetCacheDependencies(new EFCachePolicy(), ["People"], commandText);

        SortedSet<string> inUseTableNames = [$"{CacheKeyPrefix}People"];

        Assert.Equal(inUseTableNames, cacheDependencies);
    }

    [Fact]
    public void TestGetCacheDependenciesWorksForQueryHints()
    {
        const string commandText = @"SET NOCOUNT ON;
        INSERT INTO [Products] ([IsActive], [Notes], [ProductName], [ProductNumber], [UserId])
        VALUES (@p0, @p1, @p2, @p3, @p4);
        SELECT [ProductId]
        FROM [Products]
        WHERE @@ROWCOUNT = 1 AND [ProductId] = scope_identity() FOR UPDATE";

        var cacheDependencies = _efCacheDependenciesProcessor.GetCacheDependencies(new EFCachePolicy(),
            ["Posts", "Users", "Products"], commandText);

        SortedSet<string> inUseTableNames = [$"{CacheKeyPrefix}Products"];

        Assert.Equal(inUseTableNames, cacheDependencies);
    }
}