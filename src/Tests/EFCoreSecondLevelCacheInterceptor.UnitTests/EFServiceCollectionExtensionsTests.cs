using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

[TestClass]

// ReSharper disable once InconsistentNaming
public class EFServiceCollectionExtensionsTests
{
    [TestMethod]
    public void AddEFSecondLevelCache_ThrowsArgumentNullException_WhenOptionsIsNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act && Assert
        Assert.Throws<ArgumentNullException>(() => services.AddEFSecondLevelCache(null!));
    }

    [TestMethod]
    public void AddEFSecondLevelCache_RegistersRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.TryAddSingleton(typeof(ILogger<>), typeof(Logger<>));
        services.AddEFSecondLevelCache(options => options.UseMemoryCacheProvider());

        // Assert
        using var serviceProvider = services.BuildServiceProvider();

        Assert.IsNotNull(serviceProvider.GetService<IEFDebugLogger>());
        Assert.IsNotNull(serviceProvider.GetService<IEFCacheServiceCheck>());
        Assert.IsNotNull(serviceProvider.GetService<IEFCacheKeyPrefixProvider>());
        Assert.IsNotNull(serviceProvider.GetService<IEFCacheKeyProvider>());
        Assert.IsNotNull(serviceProvider.GetService<IEFCachePolicyParser>());
        Assert.IsNotNull(serviceProvider.GetService<IEFSqlCommandsProcessor>());
        Assert.IsNotNull(serviceProvider.GetService<IEFCacheDependenciesProcessor>());
        Assert.IsNotNull(serviceProvider.GetService<ILockProvider>());
        Assert.IsNotNull(serviceProvider.GetService<IMemoryCacheChangeTokenProvider>());
        Assert.IsNotNull(serviceProvider.GetService<IDbCommandInterceptorProcessor>());
        Assert.IsNotNull(serviceProvider.GetService<SecondLevelCacheInterceptor>());
    }

    [TestMethod]
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
        using var serviceProvider = services.BuildServiceProvider();

        Assert.IsInstanceOfType<XxHash64Unsafe>(serviceProvider.GetService<IEFHashProvider>());
    }

    [TestMethod]
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
        using var serviceProvider = services.BuildServiceProvider();

        Assert.IsInstanceOfType<CustomHashProvider>(serviceProvider.GetService<IEFHashProvider>());
    }

    [TestMethod]
    public void AddEFSecondLevelCache_RegistersDefaultCacheProvider_WhenCacheProviderIsNull()
    {
        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(() => services.AddEFSecondLevelCache(options =>
        {
            options.Settings.CacheProvider = null;
        }));
    }

    [TestMethod]
    public void AddEFSecondLevelCache_RegistersCustomCacheProvider_WhenCacheProviderIsNotNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddEFSecondLevelCache(options => { options.Settings.CacheProvider = typeof(CustomCacheProvider); });

        // Assert
        using var serviceProvider = services.BuildServiceProvider();

        Assert.IsInstanceOfType<CustomCacheProvider>(serviceProvider.GetService<IEFCacheServiceProvider>());
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

        public void InsertValue(EFCacheKey cacheKey, EFCachedData value, EFCachePolicy cachePolicy)
            => throw new NotImplementedException();

        public void InvalidateCacheDependencies(EFCacheKey cacheKey) => throw new NotImplementedException();
    }
}