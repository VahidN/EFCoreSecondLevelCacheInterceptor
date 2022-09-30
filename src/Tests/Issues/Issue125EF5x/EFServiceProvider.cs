using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EasyCaching.Core.Configurations;
using EFCoreSecondLevelCacheInterceptor;
using Issue125EF5x.DataLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Issue125EF5x;

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

        services.AddLogging(cfg => cfg.AddConsole().AddDebug().SetMinimumLevel(LogLevel.Debug));

        const string providerName = "Redis1";
        services.AddEasyCaching(o =>
                                {
                                    o.UseRedis(cfg =>
                                               {
                                                   cfg.SerializerName = "Pack";
                                                   cfg.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
                                                   cfg.DBConfig.AllowAdmin = true;
                                                   cfg.DBConfig.ConnectionTimeout = 10000;
                                               }, providerName);
                                    o.WithMessagePack( /*so =>
                    {
                        so.EnableCustomResolver = true;
                        so.CustomResolvers = CompositeResolver.Create(new MessagePack.IFormatterResolver[]
                        {
                            NativeDateTimeResolver.Instance,
                            ContractlessStandardResolver.Instance,
                            StandardResolverAllowPrivate.Instance
                        });
                    },*/
                                                      "Pack");
                                });
        services.AddEFSecondLevelCache(o =>
                                       {
                                           o.UseEasyCachingCoreProvider(providerName, false).DisableLogging();
                                           o.CacheAllQueries(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(10));
                                       });

        var basePath = Directory.GetCurrentDirectory();
        Console.WriteLine($"Using `{basePath}` as the ContentRootPath");
        var configuration = new ConfigurationBuilder()
                            .SetBasePath(basePath)
                            .AddJsonFile("appsettings.json", false, true)
                            .Build();
        services.AddSingleton(_ => configuration);
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
                                                    {
                                                        options
                                                            .AddInterceptors(serviceProvider
                                                                                 .GetRequiredService<
                                                                                     SecondLevelCacheInterceptor>())
                                                            .UseSqlServer(GetConnectionString(basePath, configuration))
                                                            .LogTo(sql => Console.WriteLine(sql));
                                                    });

        return services.BuildServiceProvider();
    }

    public static string GetConnectionString(string basePath, IConfigurationRoot configuration)
    {
        var testsFolder = basePath.Split(new[] { "\\Issues\\" }, StringSplitOptions.RemoveEmptyEntries)[0];
        var contentRootPath = Path.Combine(testsFolder, "Issues", "Issue125EF5x");
        var connectionString = configuration["ConnectionStrings:ApplicationDbContextConnection"];
        if (connectionString.Contains("%CONTENTROOTPATH%"))
        {
            connectionString = connectionString.Replace("%CONTENTROOTPATH%", contentRootPath);
        }

        Console.WriteLine($"Using {connectionString}");
        return connectionString;
    }
}