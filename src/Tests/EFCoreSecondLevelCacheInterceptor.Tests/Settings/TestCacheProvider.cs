namespace EFCoreSecondLevelCacheInterceptor.Tests;

public enum TestCacheProvider
{
    BuiltInInMemory,
    CacheManagerCoreInMemory,
    CacheManagerCoreRedis,
    EasyCachingCoreInMemory,
    EasyCachingCoreRedis,
    EasyCachingCoreHybrid,
    FusionCache,
    StackExchangeRedis
}