# EF Core 3.1.x Second Level Cache Interceptor

<p align="left">
  <a href="https://github.com/VahidN/EFCoreSecondLevelCacheInterceptor">
     <img alt="GitHub Actions status" src="https://github.com/VahidN/EFCoreSecondLevelCacheInterceptor/workflows/.NET%20Core%20Build/badge.svg">
  </a>
</p>

## Entity Framework Core 3.1.x Second Level Caching Library

Second level caching is a query cache. The results of EF commands will be stored in the cache, so that the same EF commands will retrieve their data from the cache rather than executing them against the database again.

## Install via NuGet

To install EFCoreSecondLevelCacheInterceptor, run the following command in the Package Manager Console:

[![Nuget](https://img.shields.io/nuget/v/EFCoreSecondLevelCacheInterceptor)](https://github.com/VahidN/EFCoreSecondLevelCacheInterceptor)

```powershell
PM> Install-Package EFCoreSecondLevelCacheInterceptor
```

You can also view the [package page](http://www.nuget.org/packages/EFCoreSecondLevelCacheInterceptor/) on NuGet.

## Usage

1- [Register the required services](/src/Tests/EFCoreSecondLevelCacheInterceptor.AspNetCoreSample/Startup.cs) of `EFCoreSecondLevelCacheInterceptor`:

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
                options.UseMemoryCacheProvider()

            // Installing Redis on Windows: http://taswar.zeytinsoft.com/intro-to-redis-for-net-developers/
            // Install the Redis binaries in the default NuGet tools directory: https://www.nuget.org/packages/Redis-64/
            // Different ways to configure Redis: https://stackexchange.github.io/StackExchange.Redis/Configuration#configuration-options
            // Redis Desktop Manager: https://github.com/uglide/RedisDesktopManager
            // options.UseRedisCacheProvider(Configuration["RedisConfiguration"])
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

If you want to use the Redis as the preferred cache provider, use `options.UseRedisCacheProvider(Configuration["RedisConfiguration"])`.

2- [Add SecondLevelCacheInterceptor](/src/Tests/EFCoreSecondLevelCacheInterceptor.Tests.DataLayer/MsSqlServiceCollectionExtensions.cs) to your `DbContextOptionsBuilder` pipeline:

```csharp
        public static void UseConfiguredMsSql(this DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            optionsBuilder.UseSqlServer(
                        connectionString,
                        sqlServerOptionsBuilder =>
                        {
                            sqlServerOptionsBuilder.CommandTimeout((int)TimeSpan.FromMinutes(3).TotalSeconds);
                            sqlServerOptionsBuilder.EnableRetryOnFailure();
                            sqlServerOptionsBuilder.MigrationsAssembly(typeof(MsSqlServiceCollectionExtensions).Assembly.FullName);
                        });
            optionsBuilder.AddInterceptors(new SecondLevelCacheInterceptor());
        }
```

3- Setting up the cache invalidation:

This library doesn't need any settings for the cache invalidation. It watches for all of the CRUD operations using its interceptor and then invalidates the related cache entries automatically.

4- To cache the results of the normal queries like:

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

NOTE: It doesn't matter where the `Cacheable` method is located in this expression tree. [It just adds](/src/EFCoreSecondLevelCacheInterceptor/EFCachedQueryExtensions.cs) the standard `TagWith` method to mark this query as `Cacheable`. Later `SecondLevelCacheInterceptor` will use this tag to identify the `Cacheable` queries.

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
                options.UseMemoryCacheProvider();
                options.CacheAllQueries(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(30));
            });

            // ...
```

This will put the whole system's queries in cache. In this case calling the `Cacheable()` methods won't be necessary. If you specify the `Cacheable()` method, its setting will override this global setting. If you want to exclude some queries from this global cache, apply the `NotCacheable()` method to them.

## Samples

- [Console App Sample](/src/Tests/EFCoreSecondLevelCacheInterceptor.ConsoleSample/)
- [ASP.NET Core App Sample](/src/Tests/EFCoreSecondLevelCacheInterceptor.AspNetCoreSample/)
- [Tests](/src/Tests/EFCoreSecondLevelCacheInterceptor.Tests/)

## Guidance

### When to use

Good candidates for query caching are global site settings and public data, such as infrequently changing articles or comments. It can also be beneficial to cache data specific to a user so long as the cache expires frequently enough relative to the size of the user base that memory consumption remains acceptable. Small, per-user data that frequently exceeds the cache's lifetime, such as a user's photo path, is better held in user claims, which are stored in cookies, than in this cache.

### Scope

This cache is scoped to the application, not the current user. It does not use session variables. Accordingly, when retrieving cached per-user data, be sure queries in include code such as `.Where(x => .... && x.UserId == id)`.

### Invalidation

This cache is updated when an entity is changed (insert, update, or delete) via a DbContext that uses this library. If the database is updated through some other means, such as a stored procedure or trigger, the cache becomes stale.
