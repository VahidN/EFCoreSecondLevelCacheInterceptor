using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EasyCaching.Core.Configurations;
using EFCoreSecondLevelCacheInterceptor;
using Issue12PostgreSql.DataLayer;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Issue12PostgreSql;

public static class EFServiceProvider
{
    private static readonly Lazy<IServiceProvider> _serviceProviderBuilder =
        new(getServiceProvider, LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    ///     A lazy loaded thread-safe singleton
    /// </summary>
    public static IServiceProvider Instance { get; } = _serviceProviderBuilder.Value;

    public static T GetRequiredService<T>() => Instance.GetRequiredService<T>();

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
        var configuration = new ConfigurationBuilder()
                            .SetBasePath(basePath)
                            .AddJsonFile("appsettings.json", false, true)
                            .Build();
        services.AddSingleton(_ => configuration);

        const string providerName = "Redis1";
        services.AddEasyCaching(o =>
                                {
                                    o.UseRedis(cfg =>
                                               {
                                                   cfg.SerializerName = "Pack";
                                                   cfg.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
                                                   cfg.DBConfig.AllowAdmin = true;
                                                   cfg.DBConfig.ConnectionTimeout = 10000;
                                               },
                                               providerName);
                                    o.WithMessagePack(so =>
                                                      {
                                                          so.EnableCustomResolver = true;
                                                          so.CustomResolvers = CompositeResolver.Create(
                                                           new IMessagePackFormatter[]
                                                           {
                                                               DBNullFormatter
                                                                   .Instance, // This is necessary for the null values
                                                           },
                                                           new IFormatterResolver[]
                                                           {
                                                               NativeDateTimeResolver.Instance,
                                                               ContractlessStandardResolver.Instance,
                                                               StandardResolverAllowPrivate.Instance,
                                                               TypelessContractlessStandardResolver.Instance,
                                                               DynamicGenericResolver.Instance,
                                                           });
                                                      },
                                                      "Pack");
                                });

        services.AddEFSecondLevelCache(o =>
                                       {
                                           o.UseEasyCachingCoreProvider(providerName).DisableLogging();
                                           o.CacheAllQueries(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(10));
                                           o.UseCacheKeyPrefix("EF_");
                                       });

        services.AddDbContext<ApplicationDbContext>((serviceProvider, optionsBuilder) =>
                                                    {
                                                        optionsBuilder
                                                            .AddInterceptors(serviceProvider
                                                                                 .GetRequiredService<
                                                                                     SecondLevelCacheInterceptor>())
                                                            .UseNpgsql(configuration
                                                                           ["ConnectionStrings:ApplicationDbContextConnection"])
                                                            .LogTo(sql => Console.WriteLine(sql));
                                                    });

        return services.BuildServiceProvider();
    }
}

public class DBNullFormatter : IMessagePackFormatter<DBNull>
{
    public static DBNullFormatter Instance = new();

    private DBNullFormatter()
    {
    }

    public void Serialize(ref MessagePackWriter writer, DBNull value, MessagePackSerializerOptions options)
    {
        writer.WriteNil();
    }

    public DBNull Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => DBNull.Value;
}