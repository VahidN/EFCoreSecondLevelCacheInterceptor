using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Assert = Xunit.Assert;

namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

// ReSharper disable once InconsistentNaming
public class EFCacheKeyProviderTests
{
    private readonly Mock<IEFCacheDependenciesProcessor> _cacheDependenciesProcessorMock;
    private readonly Mock<IEFCacheKeyPrefixProvider> _cacheKeyPrefixProviderMock;
    private readonly IEFCacheKeyProvider _cacheKeyProvider;
    private readonly Mock<IEFHashProvider> _hashProviderMock;
    private readonly Mock<IEFDebugLogger> _loggerMock;

    public EFCacheKeyProviderTests()
    {
        var cacheSettingsMock = new Mock<IOptions<EFCoreSecondLevelCacheSettings>>();
        var settings = new EFCoreSecondLevelCacheSettings();
        var cachePolicyParserMock = new Mock<IEFCachePolicyParser>();
        var keyProviderLoggerMock = new Mock<ILogger<EFCacheKeyProvider>>();

        _cacheDependenciesProcessorMock = new Mock<IEFCacheDependenciesProcessor>();
        _cacheKeyPrefixProviderMock = new Mock<IEFCacheKeyPrefixProvider>();
        _hashProviderMock = new Mock<IEFHashProvider>();
        _loggerMock = new Mock<IEFDebugLogger>();

        cacheSettingsMock.Setup(c => c.Value).Returns(settings);

        _cacheKeyProvider = new EFCacheKeyProvider(_cacheDependenciesProcessorMock.Object, cachePolicyParserMock.Object,
            _loggerMock.Object, keyProviderLoggerMock.Object, _hashProviderMock.Object,
            _cacheKeyPrefixProviderMock.Object, cacheSettingsMock.Object);
    }

    [Fact]
    public void EFCacheKeyProvider_ThrowsArgumentNullException_WhenHashProviderIsNull()
    {
        // Arrange
        var cacheDependenciesProcessorMock = new Mock<IEFCacheDependenciesProcessor>();
        var cachePolicyParserMock = new Mock<IEFCachePolicyParser>();
        var loggerMock = new Mock<IEFDebugLogger>();
        var keyProviderLoggerMock = new Mock<ILogger<EFCacheKeyProvider>>();
        var cacheKeyPrefixProviderMock = new Mock<IEFCacheKeyPrefixProvider>();
#if NET10_0 || NET9_0 || NET5_0 || NET6_0 || NET7_0 || NET8_0
        var cacheSettingsMock = new Mock<IOptions<EFCoreSecondLevelCacheSettings>>();
#endif

        // Act && Assert
        Assert.Throws<ArgumentNullException>(() => new EFCacheKeyProvider(cacheDependenciesProcessorMock.Object,
            cachePolicyParserMock.Object, loggerMock.Object, keyProviderLoggerMock.Object,

            // ReSharper disable once AssignNullToNotNullAttribute
            hashProvider: null, cacheKeyPrefixProviderMock.Object
#if NET10_0 || NET9_0 || NET5_0 || NET6_0 || NET7_0 || NET8_0
            , cacheSettingsMock.Object
#endif
        ));
    }

    [Fact]
    public void EFCacheKeyProvider_ThrowsArgumentNullException_WhenCacheSettingsIsNull()
    {
        // Arrange
#if NET10_0 || NET9_0 || NET5_0 || NET6_0 || NET7_0 || NET8_0
        var cacheDependenciesProcessorMock = new Mock<IEFCacheDependenciesProcessor>();
        var cachePolicyParserMock = new Mock<IEFCachePolicyParser>();
        var loggerMock = new Mock<IEFDebugLogger>();
        var keyProviderLoggerMock = new Mock<ILogger<EFCacheKeyProvider>>();
        var hashProviderMock = new Mock<IEFHashProvider>();
        var cacheKeyPrefixProviderMock = new Mock<IEFCacheKeyPrefixProvider>();

        // Act && Assert
        Assert.Throws<ArgumentNullException>(() => new EFCacheKeyProvider(cacheDependenciesProcessorMock.Object,
            cachePolicyParserMock.Object, loggerMock.Object, keyProviderLoggerMock.Object, hashProviderMock.Object,
            cacheKeyPrefixProviderMock.Object,

            // ReSharper disable once AssignNullToNotNullAttribute
            cacheSettings: null));
#endif
    }

    [Fact]
    public void EFCacheKeyProvider_CreatesInstanceSuccessfully()
    {
        // Arrange
        var cacheDependenciesProcessorMock = new Mock<IEFCacheDependenciesProcessor>();
        var cachePolicyParserMock = new Mock<IEFCachePolicyParser>();
        var loggerMock = new Mock<IEFDebugLogger>();
        var keyProviderLoggerMock = new Mock<ILogger<EFCacheKeyProvider>>();
        var hashProviderMock = new Mock<IEFHashProvider>();
        var cacheKeyPrefixProviderMock = new Mock<IEFCacheKeyPrefixProvider>();
#if NET10_0 || NET9_0 || NET5_0 || NET6_0 || NET7_0 || NET8_0
        var cacheSettingsMock = new Mock<IOptions<EFCoreSecondLevelCacheSettings>>();
        cacheSettingsMock.Setup(c => c.Value).Returns(new EFCoreSecondLevelCacheSettings());
#endif

        // Act
        var provider = new EFCacheKeyProvider(cacheDependenciesProcessorMock.Object, cachePolicyParserMock.Object,
            loggerMock.Object, keyProviderLoggerMock.Object, hashProviderMock.Object, cacheKeyPrefixProviderMock.Object
#if NET10_0 || NET9_0 || NET5_0 || NET6_0 || NET7_0 || NET8_0
            , cacheSettingsMock.Object
#endif
        );

        // Assert
        Assert.NotNull(provider);
    }

    [Fact]
    [SuppressMessage(category: "ReSharper", checkId: "AssignNullToNotNullAttribute")]
    public void GetEFCacheKey_ThrowsArgumentNullException_WhenContextIsNull()
    {
        // Arrange
        DbCommand command = null;
        DbContext context = null;
        EFCachePolicy cachePolicy = null;

        // Act
        void Act() => _cacheKeyProvider.GetEFCacheKey(command, context, cachePolicy);

        // Assert
        Assert.Throws<ArgumentNullException>(paramName: "context", Act);
    }

    [Fact]
    [SuppressMessage(category: "ReSharper", checkId: "AssignNullToNotNullAttribute")]
    public void GetEFCacheKey_ThrowsArgumentNullException_WhenCommandIsNull()
    {
        // Arrange
        DbCommand command = null;
        var context = Mock.Of<DbContext>();
        EFCachePolicy cachePolicy = null;

        // Act
        void Act() => _cacheKeyProvider.GetEFCacheKey(command, context, cachePolicy);

        // Assert
        Assert.Throws<ArgumentNullException>(paramName: "command", Act);
    }

    [Fact]
    public void GetEFCacheKey_ThrowsArgumentNullException_WhenCachePolicyIsNull()
    {
        // Arrange
        var command = Mock.Of<DbCommand>();
        var context = Mock.Of<DbContext>();
        EFCachePolicy cachePolicy = null;

        // Act
        // ReSharper disable once AssignNullToNotNullAttribute
        void Act() => _cacheKeyProvider.GetEFCacheKey(command, context, cachePolicy);

        // Assert
        Assert.Throws<ArgumentNullException>(paramName: "cachePolicy", Act);
    }

    [Fact]
    public void GetEFCacheKey_ReturnsExpectedCacheKey()
    {
        // Arrange
        var context = Mock.Of<DbContext>();

        var expected = new EFCacheKey(new SortedSet<string>())
        {
            KeyHash = "75BCD15",
            DbContext = context.GetType()
        };

        var commandMock = new Mock<DbCommand>();
        var cachePolicy = new EFCachePolicy().SaltKey(saltKey: "CacheSaltKey");
        var dbParameterCollectionMock = new Mock<DbParameterCollection>();
        var dbParameterMock = new Mock<DbParameter>();

        var parameters = new List<DbParameter>
        {
            dbParameterMock.Object
        };

        _hashProviderMock.Setup(x => x.ComputeHash(@"ConnectionString
=Name=""Value"",Size=2147483647,Precision=255,Scale=255,Direction=Input,SaltKey
=CacheSaltKey"))
            .Returns(value: 123456789);

        _loggerMock.Setup(x => x.IsLoggerEnabled).Returns(value: true);

        commandMock.Protected()
            .SetupGet<DbParameterCollection>(propertyName: "DbParameterCollection")
            .Returns(dbParameterCollectionMock.Object);

        using var enumerator = parameters.GetEnumerator();
        dbParameterCollectionMock.Setup(x => x.GetEnumerator()).Returns(enumerator);

        _cacheDependenciesProcessorMock.Setup(x => x.GetCacheDependencies(commandMock.Object, context, cachePolicy))
            .Returns(expected.CacheDependencies as SortedSet<string>);

        dbParameterMock.Setup(x => x.ParameterName).Returns(value: "Name");
        dbParameterMock.Setup(x => x.Value).Returns(value: "Value");
        dbParameterMock.Setup(x => x.Size).Returns(int.MaxValue);
        dbParameterMock.Setup(x => x.Precision).Returns(byte.MaxValue);
        dbParameterMock.Setup(x => x.Scale).Returns(byte.MaxValue);
        dbParameterMock.Setup(x => x.Direction).Returns(ParameterDirection.Input);

        // Act
        var actual = _cacheKeyProvider.GetEFCacheKey(commandMock.Object, context, cachePolicy);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetEFCacheKey_ReturnsExpectedCacheKeyWithPrefix()
    {
        // Arrange
        var context = Mock.Of<DbContext>();

        var expected = new EFCacheKey(new SortedSet<string>())
        {
            KeyHash = "CacheKeyPrefix75BCD15",
            DbContext = context.GetType()
        };

        var commandMock = new Mock<DbCommand>();
        var cachePolicy = new EFCachePolicy().SaltKey(saltKey: "CacheSaltKey");
        var dbParameterCollectionMock = new Mock<DbParameterCollection>();
        var dbParameterMock = new Mock<DbParameter>();

        var parameters = new List<DbParameter>
        {
            dbParameterMock.Object
        };

        _cacheKeyPrefixProviderMock.Setup(x => x.GetCacheKeyPrefix()).Returns(value: "CacheKeyPrefix");

        _hashProviderMock.Setup(x => x.ComputeHash(@"ConnectionString
=Name=""Value"",Size=2147483647,Precision=255,Scale=255,Direction=Input,SaltKey
=CacheSaltKey"))
            .Returns(value: 123456789);

        _loggerMock.Setup(x => x.IsLoggerEnabled).Returns(value: true);

        commandMock.Protected()
            .SetupGet<DbParameterCollection>(propertyName: "DbParameterCollection")
            .Returns(dbParameterCollectionMock.Object);

        using var enumerator = parameters.GetEnumerator();
        dbParameterCollectionMock.Setup(x => x.GetEnumerator()).Returns(enumerator);

        _cacheDependenciesProcessorMock.Setup(x => x.GetCacheDependencies(commandMock.Object, context, cachePolicy))
            .Returns(expected.CacheDependencies as SortedSet<string>);

        dbParameterMock.Setup(x => x.ParameterName).Returns(value: "Name");
        dbParameterMock.Setup(x => x.Value).Returns(value: "Value");
        dbParameterMock.Setup(x => x.Size).Returns(int.MaxValue);
        dbParameterMock.Setup(x => x.Precision).Returns(byte.MaxValue);
        dbParameterMock.Setup(x => x.Scale).Returns(byte.MaxValue);
        dbParameterMock.Setup(x => x.Direction).Returns(ParameterDirection.Input);

        // Act
        var actual = _cacheKeyProvider.GetEFCacheKey(commandMock.Object, context, cachePolicy);

        // Assert
        Assert.Equal(expected, actual);
    }
}