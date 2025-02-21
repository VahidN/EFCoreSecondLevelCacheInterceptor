# EF Core Second Level Cache Interceptor

[![EFCoreSecondLevelCacheInterceptor](https://github.com/VahidN/EFCoreSecondLevelCacheInterceptor/workflows/.NET%20Core%20Build/badge.svg)](https://github.com/VahidN/EFCoreSecondLevelCacheInterceptor)

Second level caching is a query cache. The results of EF commands will be stored in the cache, so that the same EF commands will retrieve their data from the cache rather than executing them against the database again.


## How to upgrade to version 5

To support more advanced caching providers, this library uses different assemblies and NuGet packages now.
To upgrade to version 5, first remove the `EFCoreSecondLevelCacheInterceptor` dependency. It doesn't have any built-in caching provider anymore. 
But you can still use it to introduce your own custom caching provider by calling its `options.UseCustomCacheProvider<T>()` method (and you won't need the new packages).
To install `EFCoreSecondLevelCacheInterceptor` as before, run the following command in the Package Manager Console:

[![Nuget](https://img.shields.io/nuget/v/EFCoreSecondLevelCacheInterceptor)](http://www.nuget.org/packages/EFCoreSecondLevelCacheInterceptor/)

```powershell
dotnet add package EFCoreSecondLevelCacheInterceptor
```

But if you were using the built-in `In-Memory` cache provider, just install this new package:

[![Nuget](https://img.shields.io/nuget/v/EFCoreSecondLevelCacheInterceptor.MemoryCache)](http://www.nuget.org/packages/EFCoreSecondLevelCacheInterceptor.MemoryCache/)
```powershell
dotnet add package EFCoreSecondLevelCacheInterceptor.MemoryCache
```

Or if you were using the `EasyCaching.Core provider`, install the new `EFCoreSecondLevelCacheInterceptor.EasyCaching.Core` package:

[![Nuget](https://img.shields.io/nuget/v/EFCoreSecondLevelCacheInterceptor.EasyCaching.Core)](http://www.nuget.org/packages/EFCoreSecondLevelCacheInterceptor.EasyCaching.Core/)
```powershell
dotnet add package EFCoreSecondLevelCacheInterceptor.EasyCaching.Core
```

Or if you were using the `CacheManager.Core provider`, install the new `EFCoreSecondLevelCacheInterceptor.CacheManager.Core` package:

[![Nuget](https://img.shields.io/nuget/v/EFCoreSecondLevelCacheInterceptor.CacheManager.Core)](http://www.nuget.org/packages/EFCoreSecondLevelCacheInterceptor.CacheManager.Core/)
```powershell
dotnet add package EFCoreSecondLevelCacheInterceptor.CacheManager.Core
```

Also there are two new caching providers available in V5:

### 1- EFCoreSecondLevelCacheInterceptor.StackExchange.Redis

This provider uses the StackExchange.Redis as a cache provider and it's preconfigured with a MessagePack serializer. To use it, first you should install its new NuGet package:

[![Nuget](https://img.shields.io/nuget/v/EFCoreSecondLevelCacheInterceptor.StackExchange.Redis)](http://www.nuget.org/packages/EFCoreSecondLevelCacheInterceptor.StackExchange.Redis/)
```powershell
dotnet add package EFCoreSecondLevelCacheInterceptor.StackExchange.Redis
```

And then you need to register its required services:

```csharp
var redisOptions = new ConfigurationOptions
                   {
                     EndPoints = new EndPointCollection
                     {
                        {
                            "127.0.0.1", 6379
                        }
                     },
                     AllowAdmin = true,
                     ConnectTimeout = 10000
                   };
				   
services.AddEFSecondLevelCache(options
                    => options.UseStackExchangeRedisCacheProvider(redisOptions, TimeSpan.FromMinutes(minutes: 5)));
```

### 2- EFCoreSecondLevelCacheInterceptor.FusionCache

This provider uses the [FusionCache](https://github.com/ZiggyCreatures/FusionCache) as a cache provider. To use it, first you should install its new NuGet package:

[![Nuget](https://img.shields.io/nuget/v/EFCoreSecondLevelCacheInterceptor.FusionCache)](http://www.nuget.org/packages/EFCoreSecondLevelCacheInterceptor.FusionCache/)
```powershell
dotnet add package EFCoreSecondLevelCacheInterceptor.FusionCache
```

And then this is how you can register its required services:

```csharp
services.AddFusionCache()
            .WithOptions(options =>
            {
                options.DefaultEntryOptions = new FusionCacheEntryOptions
                {
                    // CACHE DURATION
                    Duration = TimeSpan.FromMinutes(minutes: 1),

                    // FAIL-SAFE OPTIONS
                    IsFailSafeEnabled = true,
                    FailSafeMaxDuration = TimeSpan.FromHours(hours: 2),
                    FailSafeThrottleDuration = TimeSpan.FromSeconds(seconds: 30),

                    // FACTORY TIMEOUTS
                    FactorySoftTimeout = TimeSpan.FromMilliseconds(milliseconds: 500),
                    FactoryHardTimeout = TimeSpan.FromMilliseconds(milliseconds: 1500),

                    // DISTRIBUTED CACHE
                    DistributedCacheSoftTimeout = TimeSpan.FromSeconds(seconds: 10),
                    DistributedCacheHardTimeout = TimeSpan.FromSeconds(seconds: 20),
                    AllowBackgroundDistributedCacheOperations = true,

                    // JITTERING
                    JitterMaxDuration = TimeSpan.FromSeconds(seconds: 2)
                };

                // DISTIBUTED CACHE CIRCUIT-BREAKER
                options.DistributedCacheCircuitBreakerDuration = TimeSpan.FromSeconds(seconds: 2);

                // CUSTOM LOG LEVELS
                options.FailSafeActivationLogLevel = LogLevel.Debug;
                options.SerializationErrorsLogLevel = LogLevel.Warning;
                options.DistributedCacheSyntheticTimeoutsLogLevel = LogLevel.Debug;
                options.DistributedCacheErrorsLogLevel = LogLevel.Error;
                options.FactorySyntheticTimeoutsLogLevel = LogLevel.Debug;
                options.FactoryErrorsLogLevel = LogLevel.Error;
            });
			
services.AddEFSecondLevelCache(options => options.UseFusionCacheProvider());
```


## Usage ([1](#1--register-a-preferred-cache-provider) & [2](#2--add-secondlevelcacheinterceptor-to-your-dbcontextoptionsbuilder-pipeline) are mandatory)

### 1- [Register a preferred cache provider](https://github.com/VahidN/EFCoreSecondLevelCacheInterceptor/tree/master/src/Tests/EFCoreSecondLevelCacheInterceptor.AspNetCoreSample/Startup.cs):

You can use the following cache providers:

- [Memory provider built in this library](#using-the-built-in-in-memory-cache-provider)
- [EasyCaching.Core memory provider](#using-easycachingcore-as-the-cache-provider)
- [EasyCaching.Core dynamic cache provider](#using-easycachingcore-as-a-dynamic-cache-provider)
- [CacheManager.Core cache provider](#using-cachemanagercore-as-the-cache-provider-its-not-actively-maintained)
- [A custom cache provider](#using-a-custom-cache-provider)

#### Using the built-in In-Memory cache provider

![performance](https://raw.githubusercontent.com/VahidN/EFCoreSecondLevelCacheInterceptor/master/src/Tests/EFCoreSecondLevelCacheInterceptor.PerformanceTests/int-pref.png)

```csharp
namespace EFCoreSecondLevelCacheInterceptor.AspNetCoreSample
{
    public class Startup
    {
        private readonly string _contentRootPath;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _contentRootPath = env.ContentRootPath;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddEFSecondLevelCache(options =>
                options.UseMemoryCacheProvider().ConfigureLogging(true).UseCacheKeyPrefix("EF_")
                       // Fallback on db if the caching provider fails.
                       .UseDbCallsIfCachingProviderIsDown(TimeSpan.FromMinutes(1)))

            // Please use the `CacheManager.Core` or `EasyCaching.Redis` for the Redis cache provider.
            );

            var connectionString = Configuration["ConnectionStrings:ApplicationDbContextConnection"];
            if (connectionString.Contains("%CONTENTROOTPATH%"))
            {
                connectionString = connectionString.Replace("%CONTENTROOTPATH%", _contentRootPath);
            }
            services.AddConfiguredMsSqlDbContext(connectionString);

            services.AddControllersWithViews();
        }
    }
}
```

#### Using EasyCaching.Core as the cache provider

Here you can use the [EasyCaching.Core](https://github.com/dotnetcore/EasyCaching), as a highly configurable cache manager too.
To use its in-memory caching mechanism, add this entry to the `.csproj` file:

```xml
  <ItemGroup>
    <PackageReference Include="EasyCaching.InMemory" Version="1.6.1" />
  </ItemGroup>
```

Then register its required services:

```csharp
namespace EFSecondLevelCache.Core.AspNetCoreSample
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            const string providerName1 = "InMemory1";
            services.AddEFSecondLevelCache(options =>
                    options.UseEasyCachingCoreProvider(providerName1, isHybridCache: false).ConfigureLogging(true).UseCacheKeyPrefix("EF_")
                           // Fallback on db if the caching provider fails.
                           .UseDbCallsIfCachingProviderIsDown(TimeSpan.FromMinutes(1))
            );

            // Add an in-memory cache service provider
            // More info: https://easycaching.readthedocs.io/en/latest/In-Memory/
            services.AddEasyCaching(options =>
            {
                // use memory cache with your own configuration
                options.UseInMemory(config =>
                {
                    config.DBConfig = new InMemoryCachingOptions
                    {
                        // scan time, default value is 60s
                        ExpirationScanFrequency = 60,
                        // total count of cache items, default value is 10000
                        SizeLimit = 100,

                        // enable deep clone when reading object from cache or not, default value is true.
                        EnableReadDeepClone = false,
                        // enable deep clone when writing object to cache or not, default value is false.
                        EnableWriteDeepClone = false,
                    };
                    // the max random second will be added to cache's expiration, default value is 120
                    config.MaxRdSecond = 120;
                    // whether enable logging, default is false
                    config.EnableLogging = false;
                    // mutex key's alive time(ms), default is 5000
                    config.LockMs = 5000;
                    // when mutex key alive, it will sleep some time, default is 300
                    config.SleepMs = 300;
                }, providerName1);
            });
        }
    }
}
```

If you want to use the Redis as the preferred cache provider with `EasyCaching.Core`, first install the following package:

```xml
  <ItemGroup>
    <PackageReference Include="EasyCaching.Redis" Version="1.6.1" />
    <PackageReference Include="EasyCaching.Serialization.MessagePack" Version="1.6.1" />
  </ItemGroup>
```

And then register its required services:

```csharp
namespace EFSecondLevelCache.Core.AspNetCoreSample
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            const string providerName1 = "Redis1";
            services.AddEFSecondLevelCache(options =>
                    options.UseEasyCachingCoreProvider(providerName1, isHybridCache: false).ConfigureLogging(true).UseCacheKeyPrefix("EF_")
                           // Fallback on db if the caching provider fails (for example, if Redis is down).
                           .UseDbCallsIfCachingProviderIsDown(TimeSpan.FromMinutes(1))
            );

            // More info: https://easycaching.readthedocs.io/en/latest/Redis/
            services.AddEasyCaching(option =>
            {
                option.UseRedis(config =>
                {
                    config.DBConfig.AllowAdmin = true;
                    config.DBConfig.SyncTimeout = 10000;
                    config.DBConfig.AsyncTimeout = 10000;
                    config.DBConfig.Endpoints.Add(new EasyCaching.Core.Configurations.ServerEndPoint("127.0.0.1", 6379));
                    config.EnableLogging = true;
                    config.SerializerName = "Pack";
                    config.DBConfig.ConnectionTimeout = 10000;
                }, providerName1)
                .WithMessagePack(so =>
                                      {
                                         so.EnableCustomResolver = true;
                                         so.CustomResolvers = CompositeResolver.Create(
                                         new IMessagePackFormatter[]
                                         {
                                               DBNullFormatter.Instance, // This is necessary for the null values
                                         },
                                         new IFormatterResolver[]
                                         {
                                              NativeDateTimeResolver.Instance,
                                              ContractlessStandardResolver.Instance,
                                              StandardResolverAllowPrivate.Instance,
                                         });
                                       },
                                       "Pack");
            });
        }
    }
}
```

[Here is a sample about it](https://github.com/VahidN/EFCoreSecondLevelCacheInterceptor/tree/master/src/Tests/Issues/Issue123WithMessagePack/EFServiceProvider.cs).


#### Using EasyCaching.Core as a dynamic cache provider

If you want to support multitenancy in your application and have a different Redis database per each tenant, first register multiple pre-configured providers with known `providerName`s and then select these `providerName`s based on the current tenant this way dynamically:

```C#
services.AddEFSecondLevelCache(options =>
    options.UseEasyCachingCoreProvider(
	   (serviceProvider, cacheKey) => "redis-db-" + serviceProvider.GetRequiredService<IHttpContextAccesor>().HttpContext.Request.Headers["tenant-id"],
	   isHybridCache: false)
	// `Or` you can set the cache key prefix per tenant dynamically  
	.UseCacheKeyPrefix(serviceProvider => "EF_" + serviceProvider.GetRequiredService<IHttpContextAccesor>().HttpContext.Request.Headers["tenant-id"])
	.ConfigureLogging(true)
	.UseCacheKeyPrefix("EF_")
        // Fallback on db if the caching provider fails.
        .UseDbCallsIfCachingProviderIsDown(TimeSpan.FromMinutes(1))
);
```


#### Using CacheManager.Core as the cache provider [It's not actively maintained]

Also here you can use the [CacheManager.Core](https://github.com/MichaCo/CacheManager), as a highly configurable cache manager too.
To use its in-memory caching mechanism, add these entries to the `.csproj` file:

```xml
  <ItemGroup>
    <PackageReference Include="CacheManager.Core" Version="1.2.0" />
    <PackageReference Include="CacheManager.Microsoft.Extensions.Caching.Memory" Version="1.2.0" />
    <PackageReference Include="CacheManager.Serialization.Json" Version="1.2.0" />
  </ItemGroup>
```

Then register its required services:

```csharp
namespace EFSecondLevelCache.Core.AspNetCoreSample
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddEFSecondLevelCache(options =>
                options.UseCacheManagerCoreProvider().ConfigureLogging(true).UseCacheKeyPrefix("EF_")
                       // Fallback on db if the caching provider fails.
                       .UseDbCallsIfCachingProviderIsDown(TimeSpan.FromMinutes(1))
            );

            // Add an in-memory cache service provider
            services.AddSingleton(typeof(ICacheManager<>), typeof(BaseCacheManager<>));
            services.AddSingleton(typeof(ICacheManagerConfiguration),
                new CacheManager.Core.ConfigurationBuilder()
                        .WithJsonSerializer()
                        .WithMicrosoftMemoryCacheHandle(instanceName: "MemoryCache1")
                        .Build());
        }
    }
}
```

If you want to use the Redis as the preferred cache provider with `CacheManager.Core`, first install the `CacheManager.StackExchange.Redis` package and then register its required services:

```csharp
// Add Redis cache service provider
var jss = new JsonSerializerSettings
{
    NullValueHandling = NullValueHandling.Ignore,
    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
    TypeNameHandling = TypeNameHandling.Auto,
    Converters = { new SpecialTypesConverter() }
};

const string redisConfigurationKey = "redis";
services.AddSingleton(typeof(ICacheManagerConfiguration),
    new CacheManager.Core.ConfigurationBuilder()
        .WithJsonSerializer(serializationSettings: jss, deserializationSettings: jss)
        .WithUpdateMode(CacheUpdateMode.Up)
        .WithRedisConfiguration(redisConfigurationKey, config =>
        {
            config.WithAllowAdmin()
                .WithDatabase(0)
                .WithEndpoint("localhost", 6379)
                // Enables keyspace notifications to react on eviction/expiration of items.
                // Make sure that all servers are configured correctly and 'notify-keyspace-events' is at least set to 'Exe', otherwise CacheManager will not retrieve any events.
                // You can try 'Egx' or 'eA' value for the `notify-keyspace-events` too.
                // See https://redis.io/topics/notifications#configuration for configuration details.
                .EnableKeyspaceEvents();
        })
        .WithMaxRetries(100)
        .WithRetryTimeout(50)
        .WithRedisCacheHandle(redisConfigurationKey)
        .Build());
services.AddSingleton(typeof(ICacheManager<>), typeof(BaseCacheManager<>));

services.AddEFSecondLevelCache(options =>
    options.UseCacheManagerCoreProvider().ConfigureLogging(true).UseCacheKeyPrefix("EF_")
           // Fallback on db if the caching provider fails.
           .UseDbCallsIfCachingProviderIsDown(TimeSpan.FromMinutes(1))
);
```

[Here is](https://github.com/VahidN/EFCoreSecondLevelCacheInterceptor/tree/master/src/Tests/EFCoreSecondLevelCacheInterceptor.Tests/Settings/EFServiceProvider.cs#L21) the definition of the SpecialTypesConverter.

#### Using a custom cache provider

If you don't want to use the above cache providers, implement your custom `IEFCacheServiceProvider` and then introduce it using the `options.UseCustomCacheProvider<T>()` method.

### 2- [Add SecondLevelCacheInterceptor](https://github.com/VahidN/EFCoreSecondLevelCacheInterceptor/tree/master/src/Tests/EFCoreSecondLevelCacheInterceptor.Tests.DataLayer/MsSqlServiceCollectionExtensions.cs) to your `DbContextOptionsBuilder` pipeline:

```csharp
    public static class MsSqlServiceCollectionExtensions
    {
        public static IServiceCollection AddConfiguredMsSqlDbContext(this IServiceCollection services, string connectionString)
        {
            services.AddDbContextPool<ApplicationDbContext>((serviceProvider, optionsBuilder) =>
                    optionsBuilder
                        .UseSqlServer(
                            connectionString,
                            sqlServerOptionsBuilder =>
                            {
                                sqlServerOptionsBuilder
                                    .CommandTimeout((int)TimeSpan.FromMinutes(3).TotalSeconds)
                                    .EnableRetryOnFailure()
                                    .MigrationsAssembly(typeof(MsSqlServiceCollectionExtensions).Assembly.FullName);
                            })
                        .AddInterceptors(serviceProvider.GetRequiredService<SecondLevelCacheInterceptor>()));
            return services;
        }
    }
```

Note: Some database providers don't support special fields such as `DateTimeOffset`, `TimeSpan`, etc. For these scenarios you will need [the related converters](https://github.com/VahidN/EFCoreSecondLevelCacheInterceptor/blob/masterhttps://github.com/VahidN/EFCoreSecondLevelCacheInterceptor/tree/master/src/Tests/Issues/Issue9SQLiteInt32/DataLayer/ApplicationDbContext.cs#L35).

### 3- Setting up the cache invalidation:

This library doesn't need any settings for the cache invalidation. It watches for all of the CRUD operations using its interceptor and then invalidates the related cache entries automatically.
But if you want to invalidate the whole cache `manually`, inject the `IEFCacheServiceProvider` service and then call its `_cacheServiceProvider.ClearAllCachedEntries()` method or use it this way to specify the root cache keys which are a collection of a Prefix+TableName:
```C#
// Partial cache invalidation using the specified table names
// This is useful when you are monitoring your DB's changes using the SqlTableDependency
_cacheServiceProvider.InvalidateCacheDependencies(new EFCacheKey(new HashSet<string>()
{
   "EF_TableName1", // "EF_" is the cache key's prefix
   "EF_TableName2"
}));
```

I you want to get notified about the cache-invalidation events and involved cache dependencies, use the `NotifyCacheInvalidation` method:
```C#
services.AddEFSecondLevelCache(options =>
{
   options.UseMemoryCacheProvider(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(value: 30))
                .NotifyCacheInvalidation(invalidationInfo =>
                {
                    invalidationInfo.ServiceProvider.GetRequiredService<ILoggerFactory>()
                        .CreateLogger(categoryName: "NotifyCacheInvalidation")
                        .LogWarning(message: "{Message}",
                            invalidationInfo.ClearAllCachedEntries
                                ? "Invalidated all the cache entries!"
                                : $"Invalidated [{string.Join(separator: ", ", invalidationInfo.CacheDependencies)}] dependencies.");
                })
```


### 4- To cache the results of the normal queries like:

```csharp
var post1 = context.Posts
                   .Where(x => x.Id > 0)
                   .OrderBy(x => x.Id)
                   .FirstOrDefault();
```

We can use the new `Cacheable()` extension method:

```csharp
var post1 = context.Posts
                   .Where(x => x.Id > 0)
                   .OrderBy(x => x.Id)
                   .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                   .FirstOrDefault();  // Async methods are supported too.
```

NOTE: It doesn't matter where the `Cacheable` method is located in this expression tree. [It just adds](https://github.com/VahidN/EFCoreSecondLevelCacheInterceptor/tree/master/src/EFCoreSecondLevelCacheInterceptor/EFCachedQueryExtensions.cs) the standard `TagWith` method to mark this query as `Cacheable`. Later `SecondLevelCacheInterceptor` will use this tag to identify the `Cacheable` queries.

Also it's possible to set the `Cacheable()` method's settings globally:

```csharp
services.AddEFSecondLevelCache(options => options.UseMemoryCacheProvider(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(5)).ConfigureLogging(true)
                                          	 .UseCacheKeyPrefix("EF_")
                                                 // Fallback on db if the caching provider fails.
                                                 .UseDbCallsIfCachingProviderIsDown(TimeSpan.FromMinutes(1))
);
```

In this case the above query will become:

```csharp
var post1 = context.Posts
                   .Where(x => x.Id > 0)
                   .OrderBy(x => x.Id)
                   .Cacheable()
                   .FirstOrDefault();  // Async methods are supported too.
```

If you specify the settings of the `Cacheable()` method explicitly such as `Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))`, its setting will override the global setting.

## Caching all of the queries

To cache all of the system's queries, just set the `CacheAllQueries()` method:

```csharp
namespace EFCoreSecondLevelCacheInterceptor.AspNetCoreSample
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddEFSecondLevelCache(options =>
            {
                options.UseMemoryCacheProvider().ConfigureLogging(true).UseCacheKeyPrefix("EF_");
                options.CacheAllQueries(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(30));
                // Fallback on db if the caching provider fails.
                options.UseDbCallsIfCachingProviderIsDown(TimeSpan.FromMinutes(1));
            });

            // ...
```

This will put the whole system's queries in cache. In this case calling the `Cacheable()` methods won't be necessary. If you specify the `Cacheable()` method, its setting will override this global setting. If you want to exclude some of the queries from this global cache, apply the `NotCacheable()` method to them.

## Caching some of the queries

To cache some of the system's queries based on their entity-types or table-names, use `CacheQueriesContainingTypes` or `CacheQueriesContainingTableNames` methods:

```csharp
namespace EFCoreSecondLevelCacheInterceptor.AspNetCoreSample
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddEFSecondLevelCache(options =>
            {
                options.UseMemoryCacheProvider().ConfigureLogging(true).UseCacheKeyPrefix("EF_")
                       // Fallback on db if the caching provider fails.
                       .UseDbCallsIfCachingProviderIsDown(TimeSpan.FromMinutes(1))
                    /*.CacheQueriesContainingTypes(
                        CacheExpirationMode.Absolute, TimeSpan.FromMinutes(30), TableTypeComparison.Contains,
                        typeof(Post), typeof(Product), typeof(User)
                        )*/
                    .CacheQueriesContainingTableNames(
                        CacheExpirationMode.Absolute, TimeSpan.FromMinutes(30), TableNameComparison.ContainsOnly,
                        "posts", "products", "users"
                        );
            });

            // ...
```

This will put the the specified system's queries in cache. In this case calling the `Cacheable()` methods won't be necessary. If you specify the `Cacheable()` method, its setting will override this global setting. If you want to exclude some of the queries from this global cache, apply the `NotCacheable()` method to them.
Also you can skip caching some of the defined DbContexts using `SkipCachingDbContexts()` method.

## Skip caching of some of the queries

To skip caching some of the system's queries based on their SQL commands, set the `SkipCachingCommands` predicate:

```csharp
namespace EFCoreSecondLevelCacheInterceptor.AspNetCoreSample
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddEFSecondLevelCache(options =>
            {
                options.UseMemoryCacheProvider().ConfigureLogging(true).UseCacheKeyPrefix("EF_")
                       // Fallback on db if the caching provider fails.
                       .UseDbCallsIfCachingProviderIsDown(TimeSpan.FromMinutes(1))
                        // How to skip caching specific commands
                       .SkipCachingCommands(commandText =>
                                commandText.Contains("NEWID()", StringComparison.InvariantCultureIgnoreCase));
            });
            // ...
```

## Skip caching of some of the queries based on their results

To skip caching some of the system's queries based on their results, set the `SkipCachingResults` predicate:

```csharp
namespace EFCoreSecondLevelCacheInterceptor.AspNetCoreSample
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddEFSecondLevelCache(options =>
            {
                options.UseMemoryCacheProvider().ConfigureLogging(true).UseCacheKeyPrefix("EF_")
                        // Fallback on db if the caching provider fails.
                        .UseDbCallsIfCachingProviderIsDown(TimeSpan.FromMinutes(1))
                        // Don't cache null values. Remove this optional setting if it's not necessary.
                        .SkipCachingResults(result =>
                                result.Value == null || (result.Value is EFTableRows rows && rows.RowsCount == 0));
            });
            // ...
```

## Skip caching some of the queries based on their table names

To do not cache some of the system's queries based on their entity-types or table-names, use `CacheAllQueriesExceptContainingTypes` or `CacheAllQueriesExceptContainingTableNames` methods:

```csharp
namespace EFCoreSecondLevelCacheInterceptor.AspNetCoreSample
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddEFSecondLevelCache(options =>
            {
                options.UseMemoryCacheProvider().ConfigureLogging(true).UseCacheKeyPrefix("EF_")
                      // Fallback on db if the caching provider fails.
                      .UseDbCallsIfCachingProviderIsDown(TimeSpan.FromMinutes(1))
                    /*.CacheAllQueriesExceptContainingTypes(
                        CacheExpirationMode.Absolute, TimeSpan.FromMinutes(30),
                        typeof(Post), typeof(Product), typeof(User)
                        )*/
                    .CacheAllQueriesExceptContainingTableNames(
                        CacheExpirationMode.Absolute, TimeSpan.FromMinutes(30),
                        "posts", "products", "users"
                        );
            });

            // ...
```

This will not put the the specified system's queries in cache. In this case calling the `Cacheable()` methods won't be necessary. If you specify the `Cacheable()` method, its setting will override this global setting.

## Skip invalidating the related cache entries of a given query

Sometimes you don't want to invalidate the cache immediately, such when you are updating a post's likes or views count. In this case to skip invalidating the related cache entries of a given CRUD command, set the `SkipCacheInvalidationCommands` predicate:

```csharp
namespace EFCoreSecondLevelCacheInterceptor.AspNetCoreSample
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddEFSecondLevelCache(options =>
            {
                options.UseMemoryCacheProvider().ConfigureLogging(true).UseCacheKeyPrefix("EF_")
                    // Fallback on db if the caching provider fails.
                    .UseDbCallsIfCachingProviderIsDown(TimeSpan.FromMinutes(1))
                    .SkipCacheInvalidationCommands(commandText =>
                                // How to skip invalidating the related cache entries of this query
                                commandText.Contains("NEWID()", StringComparison.InvariantCultureIgnoreCase));
            });
            // ...
```

## Using a different hash provider  

This library uses the [XxHash64Unsafe](https://github.com/VahidN/EFCoreSecondLevelCacheInterceptor/tree/master/src/EFCoreSecondLevelCacheInterceptor/XxHash64Unsafe.cs) class to calculate the hash of a query and its parameters to produce a corresponding cache-key.
`xxHash` is an extremely fast `non-cryptographic` Hash algorithm. If you don't like it or you want to change it, just implement the `IEFHashProvider` interface and then introduce it this way:  

```csharp
namespace EFCoreSecondLevelCacheInterceptor.AspNetCoreSample
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddEFSecondLevelCache(options =>
            {
                options.UseCustomHashProvider<MyCustomHashProvider>();
                // ...                
            });

            // ...
```


## Disabling the interceptor for a while

If you want to disable this interceptor for a while, use the `.EnableCachingInterceptor(enable: false)` method. Its default value is true.


## Providing options to control the serialization behavior

The EFCacheKeyProvider class serializes parameter values of a DbCommand to JSON values. If it causes an exception in some cases, you can specify a custom JsonSerializerOptions for it using the `UseJsonSerializerOptions(options)` method.


## Does it work?!

You should enable the logging system to see the behind the scene of the caching interceptor.
First set the `ConfigureLogging(true)`:

```c#
 services.AddEFSecondLevelCache(options =>
                options.UseMemoryCacheProvider().ConfigureLogging(true).UseCacheKeyPrefix("EF_")
                       // Fallback on db if the caching provider fails.
                      .UseDbCallsIfCachingProviderIsDown(TimeSpan.FromMinutes(1))
```

And then change the log level to `Debug` in your `appsettings.json` file:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "System": "Debug",
      "Microsoft": "Debug",
      "Microsoft.Hosting.Lifetime": "Debug"
    }
  }
}
```

Or ... you can use the second optional parameter of the `ConfigureLogging` method to access the published events of this library more easily:


```c#
 .ConfigureLogging(enable: environment.IsDevelopment(), cacheableEvent: args =>
            {
                    switch (args.EventId)
                    {
                        case CacheableLogEventId.CacheHit:
                            break;
                        case CacheableLogEventId.QueryResultCached:
                            break;
                        case CacheableLogEventId.QueryResultInvalidated:
                            args.ServiceProvider.GetRequiredService<ILoggerFactory>()
                                .CreateLogger(nameof(EFCoreSecondLevelCacheInterceptor))
                                .LogWarning(message: "{EventId} -> {Message} -> {CommandText}", args.EventId,
                                    args.Message, args.CommandText);
                            break;
                        case CacheableLogEventId.CachingSkipped:
                            break;
                        case CacheableLogEventId.InvalidationSkipped:
                            break;
                        case CacheableLogEventId.CachingSystemStarted:
                            break;
                        case CacheableLogEventId.CachingError:
                            break;
                        case CacheableLogEventId.QueryResultSuppressed:
                            break;
                        case CacheableLogEventId.CacheDependenciesCalculated:
                            break;
                        case CacheableLogEventId.CachePolicyCalculated:
                            break;
                    }
            })
```

Now after running a query multiple times, you should have these logged lines:

```
Suppressed result with a TableRows[ee20d2d7-ffc7-4ff9-9484-e8d4eecde53e] from the cache[KeyHash: EB153BD4, CacheDependencies: Page.].
Using the TableRows[ee20d2d7-ffc7-4ff9-9484-e8d4eecde53e] from the cache.
```

Notes:

- Having the `Suppressed the result with the TableRows` message means the caching interceptor is working fine.
- The next `Executed DbCommand` means nothing and it always will be logged by EF.
- At the beginning there will be a lot of internal commands executed by the EF to run migrations, etc. Ignore these commands, because you will see the `Suppressed the result with the TableRows` messages for the frequently running queries.
- Also you should verify it with a real DB profiler. It will log the 1st executed query and then on the 2nd run, you won't see it anymore.

## Samples

- [Console App Sample](https://github.com/VahidN/EFCoreSecondLevelCacheInterceptor/tree/master/src/Tests/EFCoreSecondLevelCacheInterceptor.ConsoleSample/)
- [ASP.NET Core App Sample](https://github.com/VahidN/EFCoreSecondLevelCacheInterceptor/tree/master/src/Tests/EFCoreSecondLevelCacheInterceptor.AspNetCoreSample/)
- [Tests](https://github.com/VahidN/EFCoreSecondLevelCacheInterceptor/tree/master/src/Tests/EFCoreSecondLevelCacheInterceptor.Tests/)

## Guidance

### When to use

Good candidates for query caching are global site settings and public data, such as infrequently changing articles or comments. It can also be beneficial to cache data specific to a user so long as the cache expires frequently enough relative to the size of the user base that memory consumption remains acceptable. Small, per-user data that frequently exceeds the cache's lifetime, such as a user's photo path, is better held in user claims, which are stored in cookies, than in this cache.

### Scope

This cache is scoped to the application, not the current user. It does not use session variables. Accordingly, when retrieving cached per-user data, be sure queries in include code such as `.Where(x => .... && x.UserId == id)`.

### Invalidation

This cache is updated when an entity is changed (insert, update, or delete) via a DbContext that uses this library. If the database is updated through some other means, such as a stored procedure or trigger, the cache becomes stale.

### Transactions

To avoid complications, all of the queries inside an `explicit` transaction (context.Database.BeginTransaction()) will not be cached. But the cache invalidations due to its CRUD operations will occur. You can use `.AllowCachingWithExplicitTransactions(true)` setting to disable it.
