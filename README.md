# EF Core Second Level Cache Interceptor

[![EFCoreSecondLevelCacheInterceptor](https://github.com/VahidN/EFCoreSecondLevelCacheInterceptor/workflows/.NET%20Core%20Build/badge.svg)](https://github.com/VahidN/EFCoreSecondLevelCacheInterceptor)

Second-level caching is a query cache. The results of Entity Framework (EF) commands are stored in the cache so that the same EF commands will retrieve their data from the cache rather than executing them against the database again.

-----

## How to Use

Using the second-level cache involves three mandatory steps: installing a provider, registering it, and adding the interceptor to your `DbContext`.

### 1\. Install a Preferred Cache Provider

First, you need to add the main package:

[![Nuget](https://img.shields.io/nuget/v/EFCoreSecondLevelCacheInterceptor)](http://www.nuget.org/packages/EFCoreSecondLevelCacheInterceptor/)

```powershell
dotnet add package EFCoreSecondLevelCacheInterceptor
```

This library supports multiple caching providers, each available as a separate NuGet package. You must install at least one.

  * **In-Memory (Built-in)**: A simple in-memory cache provider.
    ```powershell
    dotnet add package EFCoreSecondLevelCacheInterceptor.MemoryCache
    ```
  * **StackExchange.Redis**: Uses Redis with a preconfigured MessagePack serializer.
    ```powershell
    dotnet add package EFCoreSecondLevelCacheInterceptor.StackExchange.Redis
    ```
  * **FusionCache**: Implements [FusionCache](https://github.com/ZiggyCreatures/FusionCache) as a cache provider.
    ```powershell
    dotnet add package EFCoreSecondLevelCacheInterceptor.FusionCache
    ```
  * **HybridCache**: Implements [.NET's HybridCache](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/hybrid?view=aspnetcore-9.0).
    ```powershell
    dotnet add package EFCoreSecondLevelCacheInterceptor.HybridCache
    ```
  * **EasyCaching.Core**: A provider for the [EasyCaching.Core](https://github.com/dotnetcore/EasyCaching) library.
    ```powershell
    dotnet add package EFCoreSecondLevelCacheInterceptor.EasyCaching.Core
    ```
  * **CacheManager.Core**: A provider for the [CacheManager.Core](https://github.com/MichaCo/CacheManager) library.
    ```powershell
    dotnet add package EFCoreSecondLevelCacheInterceptor.CacheManager.Core
    ```
  * **Custom**: You can also implement your own provider.

### 2\. Register the Cache Provider and Interceptor

In your `Startup.cs` or `Program.cs`, you need to register the EF Core second-level cache services and configure your chosen provider.

**Example: Using the Built-in In-Memory Provider**

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // 1. Add EF Core Second Level Cache services
    services.AddEFSecondLevelCache(options =>
        options.UseMemoryCacheProvider().ConfigureLogging(true).UseCacheKeyPrefix("EF_")
               // Fallback on db if the caching provider fails.
               .UseDbCallsIfCachingProviderIsDown(TimeSpan.FromMinutes(1))
    );

    // 2. Add your DbContext
    var connectionString = Configuration["ConnectionStrings:ApplicationDbContextConnection"];
    services.AddConfiguredMsSqlDbContext(connectionString);

    services.AddControllersWithViews();
}
```

*(For detailed configuration examples of other providers, see the [Available Cache Providers](https://www.google.com/search?q=%23available-cache-providers) section below.)*

### 3\. Add the Interceptor to Your DbContext

Modify your `DbContext` registration to add the `SecondLevelCacheInterceptor`. This service is automatically registered and available via dependency injection.

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
                    // Add the interceptor
                    .AddInterceptors(serviceProvider.GetRequiredService<SecondLevelCacheInterceptor>()));
        return services;
    }
}
```

### 4\. Make Queries Cacheable

To cache a query, use the `.Cacheable()` extension method. It can be placed anywhere in the LINQ query chain.

```csharp
var post = context.Posts
                   .Where(x => x.Id > 0)
                   .OrderBy(x => x.Id)
                   .Cacheable() // Mark this query to be cached
                   .FirstOrDefault();  // Async methods are also supported.
```

The `Cacheable()` method uses global settings by default, but you can override them for a specific query:

```csharp
var post = context.Posts
                   .Where(x => x.Id > 0)
                   .OrderBy(x => x.Id)
                   // Override global settings for this query
                   .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                   .FirstOrDefault();
```

-----

## How to Verify It's Working

To confirm that the interceptor is working, you should enable logging.

**1. Enable Logging in the Configuration**
Set `ConfigureLogging(true)` during service registration.

```csharp
services.AddEFSecondLevelCache(options =>
    options.UseMemoryCacheProvider().ConfigureLogging(true)
);
```

**2. Set Log Level to Debug**
In your `appsettings.json`, ensure the log level is set to `Debug`.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "System": "Debug",
      "Microsoft": "Debug"
    }
  }
}
```

When you run a cacheable query for the first time, the data is fetched from the database and cached. On subsequent executions, you should see log messages indicating a cache hit:

```
Suppressed result with a TableRows[ee20d2d7-ffc7-4ff9-9484-e8d4eecde53e] from the cache[KeyHash: EB153BD4, CacheDependencies: Page.].
Using the TableRows[ee20d2d7-ffc7-4ff9-9484-e8d4eecde53e] from the cache.
```

**Notes:**

  * The `Suppressed the result with the TableRows` message confirms the caching interceptor is working.
  * You will still see an `Executed DbCommand` log message from EF Core, but this is expected behavior.
  * You can also use a database profiler to verify that the query is only executed against the database on the first run.
  * For more direct access to library events, you can pass an action to the `ConfigureLogging` method. See the [Logging Events](https://www.google.com/search?q=%23logging-events) section for more details.

-----

## Caching Strategies

You can control caching globally or on a per-query basis.

### Global Caching

Instead of marking individual queries with `.Cacheable()`, you can define a global caching policy.

#### Caching All Queries

To cache every query in the application, use `CacheAllQueries()`.

```csharp
services.AddEFSecondLevelCache(options =>
{
    options.UseMemoryCacheProvider().UseCacheKeyPrefix("EF_");
    options.CacheAllQueries(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(30));
});
```

  * If you use `CacheAllQueries()`, you don't need to call `.Cacheable()` on individual queries.
  * A specific `.Cacheable()` call on a query will override the global setting.
  * To exclude a specific query from global caching, use the `.NotCacheable()` method.

#### Caching Queries by Type or Table Name

To cache queries that involve specific entity types or database tables, use `CacheQueriesContainingTypes` or `CacheQueriesContainingTableNames`.

```csharp
services.AddEFSecondLevelCache(options =>
{
    options.UseMemoryCacheProvider()
           .CacheQueriesContainingTableNames(
               CacheExpirationMode.Absolute, TimeSpan.FromMinutes(30), TableNameComparison.ContainsOnly,
               "posts", "products", "users"
            );
});
```

  * You can also exclude specific types or table names from caching using `CacheAllQueriesExceptContainingTypes` or `CacheAllQueriesExceptContainingTableNames`.

-----

## Cache Invalidation

This library automatically invalidates cache entries when it detects CRUD operations (via its interceptor).

### Automatic Invalidation

When you use `SaveChanges()` or `SaveChangesAsync()`, the interceptor identifies which tables have been modified and invalidates all cached queries that depend on those tables. No additional configuration is needed.

**Limitation: `ExecuteUpdate` and `ExecuteDelete`**
EF Core does not trigger interceptors for bulk operations like `ExecuteUpdate` and `ExecuteDelete` for performance reasons. These methods execute raw SQL directly, bypassing EF Core's change tracking and related events. Therefore, cache invalidation will **not** happen automatically for these operations. You must invalidate the cache manually.

### Manual Invalidation

You can manually invalidate the cache by injecting the `IEFCacheServiceProvider`.

  * **Invalidate the entire cache:**

    ```csharp
    _cacheServiceProvider.ClearAllCachedEntries();
    ```

  * **Invalidate cache entries related to specific tables:**
    This is useful if you are using an external tool like `SqlTableDependency` to monitor database changes.

    ```csharp
    // The prefix "EF_" should match your configured UseCacheKeyPrefix.
    var tableNames = new HashSet<string> { "EF_TableName1", "EF_TableName2" };
    _cacheServiceProvider.InvalidateCacheDependencies(new EFCacheKey(tableNames));
    ```

### Invalidation Notifications

To receive notifications when cache entries are invalidated, use the `NotifyCacheInvalidation` method.

```csharp
services.AddEFSecondLevelCache(options =>
{
   options.UseMemoryCacheProvider()
          .NotifyCacheInvalidation(invalidationInfo =>
          {
              var logger = invalidationInfo.ServiceProvider
                                           .GetRequiredService<ILoggerFactory>()
                                           .CreateLogger("NotifyCacheInvalidation");
              var message = invalidationInfo.ClearAllCachedEntries
                  ? "Invalidated all cache entries!"
                  : $"Invalidated dependencies: [{string.Join(", ", invalidationInfo.CacheDependencies)}]";
              logger.LogWarning(message);
          });
});
```

-----

## Advanced Configuration

### Skipping Caching

You can define rules to skip caching for certain queries based on their command text or results.

  * **Skip by Command Text:**
    ```csharp
    services.AddEFSecondLevelCache(options =>
    {
        options.SkipCachingCommands(commandText =>
            commandText.Contains("NEWID()", StringComparison.InvariantCultureIgnoreCase));
    });
    ```
  * **Skip by Result:**
    ```csharp
    services.AddEFSecondLevelCache(options =>
    {
        // Don't cache null values or empty result sets.
        options.SkipCachingResults(result =>
            result.Value == null || (result.Value is EFTableRows rows && rows.RowsCount == 0));
    });
    ```

Or if you don't want to use the **`NotCacheable()`** extension method, you can use **query tags** to attach comments to your SQL queries directly from your EF Core code. These tags are then included in the generated SQL, allowing you to create custom rules for caching behavior.

To add a query tag, use the **`.TagWith()`** extension method on your EF Core query:

```csharp
var blogs = await context.Blogs
    .TagWith("Fetching data")
    .Where(b => b.IsActive)
    .ToListAsync();
```

The tag, prepended with `--`, will be included in the generated SQL query:

```sql
-- Fetching data
SELECT * FROM Blogs WHERE IsActive = 1;
```

Now you can use query tags to define rules for when to skip caching. This is done by configuring your cache service to check for specific text in the command.

In your `Startup.cs` or `Program.cs` file, configure the cache to skip commands that contain your tag. The `SkipCachingCommands` method accepts a predicate that evaluates the command text.

```csharp
services.AddEFSecondLevelCache(options =>
{
    options.SkipCachingCommands(commandText =>
        commandText.Contains("-- Fetching data", StringComparison.InvariantCultureIgnoreCase));
});
```

With this configuration, any query tagged with `"Fetching data"` will not be cached, giving you granular control over your caching strategy.

### Skipping Invalidation

In some cases, you may want to prevent a command from invalidating the cache, such as when updating a view counter.

```csharp
services.AddEFSecondLevelCache(options =>
{
    options.SkipCacheInvalidationCommands(commandText =>
        // Assumes a command that only updates a post's view count contains this text.
        commandText.Contains("UPDATE [Posts] SET [Views]", StringComparison.InvariantCultureIgnoreCase));
});
```

### Overriding Cache Policy

Use `OverrideCachePolicy()` to dynamically change the caching policy for a query at runtime.

```csharp
services.AddEFSecondLevelCache(options =>
{
    options.OverrideCachePolicy(context =>
    {
        // Don't cache any CRUD commands
        if (context.IsCrudCommand)
        {
            return null;
        }

        // Use a "never remove" policy for queries on the 'posts' table
        if (context.CommandTableNames.Contains("posts"))
        {
            return new EFCachePolicy().ExpirationMode(CacheExpirationMode.NeverRemove);
        }

        // Use the default calculated policy for all other queries
        return null;
    });
});
```

### Logging Events

You can subscribe to caching events directly instead of parsing log files.

```csharp
.ConfigureLogging(enable: environment.IsDevelopment(), cacheableEvent: args =>
{
    switch (args.EventId)
    {
        case CacheableLogEventId.CacheHit:
            break;
        case CacheableLogEventId.QueryResultCached:
            break;
        case CacheableLogEventId.QueryResultInvalidated:
            // Example of logging a specific event
            args.ServiceProvider.GetRequiredService<ILoggerFactory>()
                .CreateLogger(nameof(EFCoreSecondLevelCacheInterceptor))
                .LogWarning("{EventId} -> {Message} -> {CommandText}",
                            args.EventId, args.Message, args.CommandText);
            break;
        // ... handle other events
    }
})
```

### Other Configurations

  * **Custom Hash Provider**: Replace the default `xxHash` algorithm by implementing `IEFHashProvider` and registering it with `options.UseCustomHashProvider<T>()`.
  * **Custom JsonSerializerOptions**: Control serialization behavior by passing `JsonSerializerOptions` to `options.UseJsonSerializerOptions(options)`.
  * **Disable Interceptor**: Temporarily disable the interceptor via `options.EnableCachingInterceptor(false)`.
  * **Skip DbContexts**: Exclude certain `DbContext` types from caching with `options.SkipCachingDbContexts()`.

-----

## Available Cache Providers

Below are setup examples for the various supported cache providers.

#### 1\. EFCoreSecondLevelCacheInterceptor.StackExchange.Redis

This provider uses `StackExchange.Redis` and is preconfigured with a MessagePack serializer.

```csharp
var redisOptions = new ConfigurationOptions
{
     EndPoints = { { "127.0.0.1", 6379 } },
     AllowAdmin = true,
     ConnectTimeout = 10000
};

services.AddEFSecondLevelCache(options =>
    options.UseStackExchangeRedisCacheProvider(redisOptions, TimeSpan.FromMinutes(5)));
```

#### 2\. EFCoreSecondLevelCacheInterceptor.FusionCache

This provider uses [FusionCache](https://github.com/ZiggyCreatures/FusionCache).

```csharp
// 1. Add FusionCache services with desired options
services.AddFusionCache()
        .WithOptions(options =>
        {
            options.DefaultEntryOptions = new FusionCacheEntryOptions
            {
                Duration = TimeSpan.FromMinutes(1),
                IsFailSafeEnabled = true,
                FailSafeMaxDuration = TimeSpan.FromHours(2),
                // ... other FusionCache options
            };
        });

// 2. Add the EF Core Caching provider
services.AddEFSecondLevelCache(options => options.UseFusionCacheProvider());
```

#### 3\. EFCoreSecondLevelCacheInterceptor.HybridCache

This provider uses the new [.NET HybridCache](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/hybrid?view=aspnetcore-9.0).

```csharp
services.AddEFSecondLevelCache(options => options.UseHybridCacheProvider());
```

#### 4\. Using EasyCaching.Core

This allows you to use `EasyCaching.Core` as a highly configurable cache manager.

  * **In-Memory with EasyCaching.Core**:
    ```csharp
    const string providerName = "InMemory1";
    services.AddEFSecondLevelCache(options =>
        options.UseEasyCachingCoreProvider(providerName, isHybridCache: false)
               .UseCacheKeyPrefix("EF_")
    );

    // Add EasyCaching.Core in-memory provider
    services.AddEasyCaching(options =>
    {
        options.UseInMemory(config =>
        {
            config.DBConfig = new InMemoryCachingOptions { SizeLimit = 10000 };
            config.MaxRdSecond = 120;
        }, providerName);
    });
    ```
  * **Redis with EasyCaching.Core**:
    First, install the required packages: `EasyCaching.Redis` and `EasyCaching.Serialization.MessagePack`.
    ```csharp
    const string providerName = "Redis1";
    services.AddEFSecondLevelCache(options =>
        options.UseEasyCachingCoreProvider(providerName, isHybridCache: false)
               .UseCacheKeyPrefix("EF_")
    );

    // Add EasyCaching.Core Redis provider
    services.AddEasyCaching(option =>
    {
        option.UseRedis(config =>
        {
            config.DBConfig.Endpoints.Add(new EasyCaching.Core.Configurations.ServerEndPoint("127.0.0.1", 6379));
            config.SerializerName = "Pack";
        }, providerName)
        .WithMessagePack("Pack"); // Configure MessagePack serializer
    });
    ```
  * **Dynamic Provider with EasyCaching.Core for Multi-tenancy**:
    You can dynamically select a cache provider based on the current context, such as a tenant ID from an HTTP request header.
    ```csharp
    services.AddEFSecondLevelCache(options =>
        options.UseEasyCachingCoreProvider(
           (serviceProvider, cacheKey) => "redis-db-" + serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext.Request.Headers["tenant-id"],
           isHybridCache: false)
    );
    ```

#### 5\. Using CacheManager.Core

This allows you to use `CacheManager.Core` as a cache manager. Note: This library is not actively maintained.

  * **In-Memory with CacheManager.Core**:
    First, install: `CacheManager.Core`, `CacheManager.Microsoft.Extensions.Caching.Memory`, and `CacheManager.Serialization.Json`.
    ```csharp
    services.AddEFSecondLevelCache(options => options.UseCacheManagerCoreProvider());

    // Add CacheManager.Core services
    services.AddSingleton(typeof(ICacheManager<>), typeof(BaseCacheManager<>));
    services.AddSingleton(typeof(ICacheManagerConfiguration),
        new CacheManager.Core.CacheConfigurationBuilder()
                .WithJsonSerializer()
                .WithMicrosoftMemoryCacheHandle("MemoryCache1")
                .Build());
    ```
  * **Redis with CacheManager.Core**:
    First, install the `CacheManager.StackExchange.Redis` package.
    ```csharp
    services.AddEFSecondLevelCache(options => options.UseCacheManagerCoreProvider());

    // Add CacheManager.Core Redis services
    const string redisConfigurationKey = "redis";
    services.AddSingleton(typeof(ICacheManagerConfiguration),
        new CacheManager.Core.CacheConfigurationBuilder()
            .WithJsonSerializer()
            .WithUpdateMode(CacheUpdateMode.Up)
            .WithRedisConfiguration(redisConfigurationKey, config =>
            {
                config.WithAllowAdmin()
                      .WithDatabase(0)
                      .WithEndpoint("localhost", 6379)
                      .EnableKeyspaceEvents();
            })
            .WithRedisCacheHandle(redisConfigurationKey)
            .Build());
    services.AddSingleton(typeof(ICacheManager<>), typeof(BaseCacheManager<>));
    ```

#### 6\. Using a Custom Cache Provider

If the provided cache providers don't meet your needs, you can implement the `IEFCacheServiceProvider` interface and then register it using the `options.UseCustomCacheProvider<T>()` method.

-----

## Guidance and Best Practices

### When to Use

Good candidates for query caching are global site settings and public data, such as infrequently changing articles or comments. It can also be beneficial to cache data specific to a user, so long as the cache expires frequently enough relative to the size of the user base that memory consumption remains acceptable. Small, per-user data that changes frequently is better held in other stores like user claims, which are stored in cookies, than in this cache.

### Scope

This cache is scoped to the application, not the current user. It does not use session variables. Accordingly, when retrieving cached per-user data, be sure your queries include a filter for the user ID, such as `.Where(x => x.UserId == id)`.

### Invalidation

The cache is updated when an entity is changed (inserted, updated, or deleted) via a `DbContext` that uses this interceptor. If the database is modified through other means, such as a stored procedure, a trigger, or another application, the cache will become stale.

### Transactions

To avoid complications, all queries inside an explicit transaction (`context.Database.BeginTransaction()`) will **not** be cached by default. However, cache invalidation for CRUD operations within the transaction will still occur. You can override this behavior and allow caching within explicit transactions by using the `.AllowCachingWithExplicitTransactions(true)` setting.

### Database Provider Compatibility

Some database providers do not natively support special data types such as `DateTimeOffset` or `TimeSpan`. For these scenarios, you will need to configure the appropriate [value converters](https://www.google.com/search?q=https://github.com/VahidN/EFCoreSecondLevelCacheInterceptor/blob/master/src/Tests/Issues/Issue9SQLiteInt32/DataLayer/ApplicationDbContext.cs%23L35) in your `DbContext`.

-----

## How to Upgrade to Version 5

To support more advanced caching providers, this library was split into multiple assemblies and NuGet packages in version 5.

1.  **Remove the old dependency**: `EFCoreSecondLevelCacheInterceptor`.
2.  **Add the new main package**:
    ```powershell
    dotnet add package EFCoreSecondLevelCacheInterceptor
    ```
3.  **Add a provider package**: The main package no longer includes a built-in provider. You must install one of the new provider packages.
      * For the previous built-in **In-Memory** cache, install:
        ```powershell
        dotnet add package EFCoreSecondLevelCacheInterceptor.MemoryCache
        ```
      * For the previous **EasyCaching.Core** provider, install:
        ```powershell
        dotnet add package EFCoreSecondLevelCacheInterceptor.EasyCaching.Core
        ```
      * For the previous **CacheManager.Core** provider, install:
        ```powershell
        dotnet add package EFCoreSecondLevelCacheInterceptor.CacheManager.Core
        ```

If you were using a custom cache provider via `options.UseCustomCacheProvider<T>()`, you do not need to install a new provider package.

-----

## Samples

  * [Console App Sample](https://github.com/VahidN/EFCoreSecondLevelCacheInterceptor/tree/master/src/Tests/EFCoreSecondLevelCacheInterceptor.ConsoleSample/)
  * [ASP.NET Core App Sample](https://github.com/VahidN/EFCoreSecondLevelCacheInterceptor/tree/master/src/Tests/EFCoreSecondLevelCacheInterceptor.AspNetCoreSample/)
  * [Project Tests](https://github.com/VahidN/EFCoreSecondLevelCacheInterceptor/tree/master/src/Tests/EFCoreSecondLevelCacheInterceptor.Tests/)
