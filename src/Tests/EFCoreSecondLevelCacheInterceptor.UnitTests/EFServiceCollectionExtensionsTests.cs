using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

// ReSharper disable once InconsistentNaming
public class EFServiceCollectionExtensionsTests
{
    [Fact]
    public void AddEFSecondLevelCache_ThrowsArgumentNullException_WhenOptionsIsNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act && Assert
        Assert.Throws<ArgumentNullException>(() => services.AddEFSecondLevelCache(null!));
    }

    [Fact]
    public void AddEFSecondLevelCache_RegistersRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.TryAddSingleton(typeof(ILogger<>), typeof(Logger<>));
        services.AddEFSecondLevelCache(options => options.UseMemoryCacheProvider());

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        Assert.NotNull(serviceProvider.GetService<IEFDebugLogger>());
        Assert.NotNull(serviceProvider.GetService<IEFCacheServiceCheck>());
        Assert.NotNull(serviceProvider.GetService<IEFCacheKeyPrefixProvider>());
        Assert.NotNull(serviceProvider.GetService<IEFCacheKeyProvider>());
        Assert.NotNull(serviceProvider.GetService<IEFCachePolicyParser>());
        Assert.NotNull(serviceProvider.GetService<IEFSqlCommandsProcessor>());
        Assert.NotNull(serviceProvider.GetService<IEFCacheDependenciesProcessor>());
        Assert.NotNull(serviceProvider.GetService<ILockProvider>());
        Assert.NotNull(serviceProvider.GetService<IMemoryCacheChangeTokenProvider>());
        Assert.NotNull(serviceProvider.GetService<IDbCommandInterceptorProcessor>());
        Assert.NotNull(serviceProvider.GetService<SecondLevelCacheInterceptor>());
    }

    [Fact]
    public void AddEFSecondLevelCache_RegistersDefaultHashProvider_WhenHashProviderIsNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddEFSecondLevelCache(options =>
        {
            options.UseMemoryCacheProvider();
            options.Settings.HashProvider = null;
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        Assert.IsType<XxHash64Unsafe>(serviceProvider.GetService<IEFHashProvider>());
    }

    [Fact]
    public void AddEFSecondLevelCache_RegistersCustomHashProvider_WhenHashProviderIsNotNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddEFSecondLevelCache(options =>
        {
            options.UseMemoryCacheProvider();
            options.Settings.HashProvider = typeof(CustomHashProvider);
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        Assert.IsType<CustomHashProvider>(serviceProvider.GetService<IEFHashProvider>());
    }

    [Fact]
    public void AddEFSecondLevelCache_RegistersDefaultCacheProvider_WhenCacheProviderIsNull()
    {
        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(() => services.AddEFSecondLevelCache(options =>
        {
            options.Settings.CacheProvider = null;
        }));
    }

    [Fact]
    public void AddEFSecondLevelCache_RegistersCustomCacheProvider_WhenCacheProviderIsNotNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddEFSecondLevelCache(options => { options.Settings.CacheProvider = typeof(CustomCacheProvider); });

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        Assert.IsType<CustomCacheProvider>(serviceProvider.GetService<IEFCacheServiceProvider>());
    }

    private class Logger<TCategoryName> : ILogger<TCategoryName>
    {
        public void Log<TState>(LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
            => throw new NotImplementedException();

        public bool IsEnabled(LogLevel logLevel) => throw new NotImplementedException();

        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
            => throw new NotImplementedException();
    }

    private class CustomHashProvider : IEFHashProvider
    {
        public ulong ComputeHash(string data) => throw new NotImplementedException();

        public ulong ComputeHash(byte[] data) => throw new NotImplementedException();

        public ulong ComputeHash(byte[] data, int offset, int len, uint seed) => throw new NotImplementedException();
    }

    private class CustomCacheProvider : IEFCacheServiceProvider
    {
        public void ClearAllCachedEntries() => throw new NotImplementedException();

        public EFCachedData GetValue(EFCacheKey cacheKey, EFCachePolicy cachePolicy)
            => throw new NotImplementedException();

        public void InsertValue(EFCacheKey cacheKey, EFCachedData? value, EFCachePolicy cachePolicy)
            => throw new NotImplementedException();

        public void InvalidateCacheDependencies(EFCacheKey cacheKey) => throw new NotImplementedException();
    }
}