using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EFCoreSecondLevelCacheInterceptor.Tests;

[TestClass]
public static class Bootstrapper
{
    [AssemblyInitialize]
    public static void Initialize(TestContext context)
    {
        clearAllCachedEntries();
        startDb();
    }

    [AssemblyCleanup]
    public static void AssemblyCleanup()
    {
        // Method intentionally left empty.
    }

    private static void clearAllCachedEntries()
    {
        try
        {
            EFServiceProvider.GetCacheServiceProvider(TestCacheProvider.CacheManagerCoreRedis).ClearAllCachedEntries();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private static void startDb()
    {
#pragma warning disable IDISP001
        var serviceProvider = EFServiceProvider.GetConfiguredContextServiceProvider(TestCacheProvider.BuiltInInMemory,
            LogLevel.Debug, cacheAllQueries: false);
#pragma warning restore IDISP001

        var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        serviceScope.Initialize();
        serviceScope.SeedData();
    }
}