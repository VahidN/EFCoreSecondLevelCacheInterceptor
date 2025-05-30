using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

public class EFCachePolicyParserTests
{
    private readonly IEFCachePolicyParser _efCachePolicyParser;

    public EFCachePolicyParserTests()
    {
        var cacheSettingsMock = new Mock<IOptions<EFCoreSecondLevelCacheSettings>>();

        cacheSettingsMock.Setup(c => c.Value)
            .Returns(new EFCoreSecondLevelCacheSettings
            {
                EnableLogging = false
            });

        var logger = new Mock<IEFDebugLogger>().Object;
        var cachePolicyParserLogger = new Mock<ILogger<EFCachePolicyParser>>().Object;
        var sqlCommandsProcessor = new EFSqlCommandsProcessor(new XxHash64Unsafe());

        _efCachePolicyParser = new EFCachePolicyParser(cacheSettingsMock.Object, sqlCommandsProcessor, logger,
            cachePolicyParserLogger);
    }

    [Fact]
    public void TestGetEFCachePolicyWith2Parts()
    {
        const string commandText = @"-- EFCachePolicy[Index(27)] --> Absolute|00:45:00

      SELECT TOP(1) [p].[Id], [p].[Title], [p].[UserId], [p].[post_type], [u].[Id], [u].[Name], [u].[UserStatus]
      FROM [Posts] AS [p]
      INNER JOIN [Users] AS [u] ON [p].[UserId] = [u].[Id]
      WHERE [p].[post_type] IN (N'post_base', N'post_page') AND ([p].[Id] > @__param1_0)
      ORDER BY [p].[Id]";

        var (cachePolicy, _) = _efCachePolicyParser.GetEFCachePolicy(commandText, allEntityTypes: null);

        Assert.Equal(CacheExpirationMode.Absolute, cachePolicy.CacheExpirationMode);
        Assert.Equal(TimeSpan.FromMinutes(value: 45), cachePolicy.CacheTimeout);
    }

    [Fact]
    public void TestGetEFCachePolicyWithNullTimeoutParts()
    {
        const string commandText = @"-- EFCachePolicy[Index(27)] --> NeverRemove|

      SELECT TOP(1) [p].[Id], [p].[Title], [p].[UserId], [p].[post_type], [u].[Id], [u].[Name], [u].[UserStatus]
      FROM [Posts] AS [p]
      INNER JOIN [Users] AS [u] ON [p].[UserId] = [u].[Id]
      WHERE [p].[post_type] IN (N'post_base', N'post_page') AND ([p].[Id] > @__param1_0)
      ORDER BY [p].[Id]";

        var (cachePolicy, _) = _efCachePolicyParser.GetEFCachePolicy(commandText, allEntityTypes: null);

        Assert.Equal(CacheExpirationMode.NeverRemove, cachePolicy.CacheExpirationMode);
        Assert.Null(cachePolicy.CacheTimeout);
    }

    [Fact]
    public void TestGetEFCachePolicyWithAdditionalTagComments()
    {
        const string commandText = @"-- CustomTagAbove

-- EFCachePolicy[Index(27)] --> Absolute|00:45:00

-- CustomTagBelow

SELECT TOP(1) [p].[Id], [p].[Title], [p].[UserId], [p].[post_type], [u].[Id], [u].[Name], [u].[UserStatus]
FROM [Posts] AS [p]
INNER JOIN [Users] AS [u] ON [p].[UserId] = [u].[Id]
WHERE [p].[post_type] IN (N'post_base', N'post_page') AND ([p].[Id] > @__param1_0)
ORDER BY [p].[Id]";

        var (cachePolicy, _) = _efCachePolicyParser.GetEFCachePolicy(commandText, allEntityTypes: null);

        Assert.Equal(CacheExpirationMode.Absolute, cachePolicy.CacheExpirationMode);
        Assert.Equal(TimeSpan.FromMinutes(value: 45), cachePolicy.CacheTimeout);
    }

    [Fact]
    public void TestGetEFCachePolicyWithAllParts()
    {
        var commandText = "-- " + EFCachePolicy.Configure(options
            => options.ExpirationMode(CacheExpirationMode.Absolute)
                .Timeout(TimeSpan.FromMinutes(value: 45))
                .SaltKey(saltKey: "saltKey")
                .CacheDependencies("item 1", "item 2"));

        var (cachePolicy, _) = _efCachePolicyParser.GetEFCachePolicy(commandText, allEntityTypes: null);

        Assert.NotNull(cachePolicy);
        Assert.Equal(CacheExpirationMode.Absolute, cachePolicy.CacheExpirationMode);
        Assert.Equal(TimeSpan.FromMinutes(value: 45), cachePolicy.CacheTimeout);
        Assert.Equal(expected: "saltKey", cachePolicy.CacheSaltKey);

        Assert.Equal(new SortedSet<string>
        {
            "item 1",
            "item 2"
        }, cachePolicy.CacheItemsDependencies as SortedSet<string>);
    }

    [Fact]
    public void TestGetEFCachePolicyWithNullParts()
    {
        var commandText = "-- " + EFCachePolicy.Configure(options
            => options.ExpirationMode(CacheExpirationMode.NeverRemove)
                .Timeout(timeout: null)
                .SaltKey(saltKey: "saltKey")
                .CacheDependencies("item 1", "item 2"));

        var (cachePolicy, _) = _efCachePolicyParser.GetEFCachePolicy(commandText, allEntityTypes: null);

        Assert.NotNull(cachePolicy);
        Assert.Equal(CacheExpirationMode.NeverRemove, cachePolicy.CacheExpirationMode);
        Assert.Null(cachePolicy.CacheTimeout);
        Assert.Equal(expected: "saltKey", cachePolicy.CacheSaltKey);

        Assert.Equal(new SortedSet<string>
        {
            "item 1",
            "item 2"
        }, cachePolicy.CacheItemsDependencies as SortedSet<string>);
    }

    [Fact]
    public void TestRemoveEFCachePolicyTagWithAdditionalTagComments()
    {
        const string commandText = @"-- CustomTagAbove

-- EFCachePolicy[Index(27)] --> Absolute|00:45:00

-- CustomTagBelow

SELECT TOP(1) [p].[Id], [p].[Title], [p].[UserId], [p].[post_type], [u].[Id], [u].[Name], [u].[UserStatus]
FROM [Posts] AS [p]
INNER JOIN [Users] AS [u] ON [p].[UserId] = [u].[Id]
WHERE [p].[post_type] IN (N'post_base', N'post_page') AND ([p].[Id] > @__param1_0)
ORDER BY [p].[Id]";

        var commandTextWithCachePolicyTagRemoved = _efCachePolicyParser.RemoveEFCachePolicyTag(commandText);

        var expectedResult = @"-- CustomTagAbove

-- CustomTagBelow

SELECT TOP(1) [p].[Id], [p].[Title], [p].[UserId], [p].[post_type], [u].[Id], [u].[Name], [u].[UserStatus]
FROM [Posts] AS [p]
INNER JOIN [Users] AS [u] ON [p].[UserId] = [u].[Id]
WHERE [p].[post_type] IN (N'post_base', N'post_page') AND ([p].[Id] > @__param1_0)
ORDER BY [p].[Id]";

        Assert.Equal(expectedResult, commandTextWithCachePolicyTagRemoved);
    }
}