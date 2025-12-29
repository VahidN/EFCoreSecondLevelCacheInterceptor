using EasyCaching.Core.Configurations;
using EFCoreSecondLevelCacheInterceptor;
using Issue12PostgreSql.DataLayer;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Issue12PostgreSql;

public static class EFServiceProvider
{
    private static readonly Lazy<IServiceProvider> _serviceProviderBuilder =
        new(getServiceProvider, LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    ///     A lazy loaded thread-safe singleton
    /// </summary>
    public static IServiceProvider Instance { get; } = _serviceProviderBuilder.Value;

    public static T GetRequiredService<T>()
        where T : notnull
        => Instance.GetRequiredService<T>();

    public static void RunInContext(Action<ApplicationDbContext> action)
    {
        using var serviceScope = GetRequiredService<IServiceScopeFactory>().CreateScope();
        using var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        action(context);
    }

    public static async Task RunInContextAsync(Func<ApplicationDbContext, Task> action)
    {
        using var serviceScope = GetRequiredService<IServiceScopeFactory>().CreateScope();
        using var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await action(context);
    }

    private static IServiceProvider getServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddOptions();

        services.AddLogging(cfg => cfg.AddConsole().AddDebug());

        var basePath = Directory.GetCurrentDirectory();
        Console.WriteLine($"Using `{basePath}` as the ContentRootPath");

        var configuration = new ConfigurationBuilder().SetBasePath(basePath)
            .AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        services.AddSingleton(_ => configuration);

        const string providerName = "Redis1";

        services.AddEasyCaching(o =>
        {
            o.UseRedis(cfg =>
            {
                cfg.SerializerName = "Pack";
                cfg.DBConfig.Endpoints.Add(new ServerEndPoint(host: "127.0.0.1", port: 6379));
                cfg.DBConfig.AllowAdmin = true;
                cfg.DBConfig.ConnectionTimeout = 10000;
            }, providerName);

            o.WithMessagePack(so =>
            {
                so.EnableCustomResolver = true;

                so.CustomResolvers = CompositeResolver.Create(new IMessagePackFormatter[]
                {
                    DBNullFormatter.Instance // This is necessary for the null values
                }, new IFormatterResolver[]
                {
                    NativeDateTimeResolver.Instance, ContractlessStandardResolver.Instance,
                    StandardResolverAllowPrivate.Instance, TypelessContractlessStandardResolver.Instance,
                    DynamicGenericResolver.Instance
                });
            }, name: "Pack");
        });

        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromHours(hours: 1),
                LocalCacheExpiration = TimeSpan.FromHours(hours: 1)
            };
        });

        services.AddEFSecondLevelCache(o =>
        {
            o.UseHybridCacheProvider(CacheExpirationMode.Absolute, TimeSpan.FromHours(hours: 1))

                //.UseMemoryCacheProvider()
                //.UseEasyCachingCoreProvider(providerName).ConfigureLogging(enable: true)
                .CacheAllQueries(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 10))
                .UseCacheKeyPrefix(prefix: "EF_")

                // Fallback on db if the caching provider (redis) is down.
                .UseDbCallsIfCachingProviderIsDown(TimeSpan.FromMinutes(minutes: 1));
        });

#pragma warning disable IDISP001
        var dataSource =
            new NpgsqlDataSourceBuilder(configuration[key: "ConnectionStrings:ApplicationDbContextConnection"])
                .EnableDynamicJson()
                .Build();
#pragma warning restore IDISP001

        services.AddDbContext<ApplicationDbContext>((serviceProvider, optionsBuilder) =>
        {
            optionsBuilder.AddInterceptors(serviceProvider.GetRequiredService<SecondLevelCacheInterceptor>())
                .UseNpgsql(dataSource);
        });

        return services.BuildServiceProvider();
    }
}