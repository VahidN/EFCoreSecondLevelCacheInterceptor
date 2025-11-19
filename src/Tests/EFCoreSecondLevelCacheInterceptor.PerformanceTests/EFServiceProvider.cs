using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EFCoreSecondLevelCacheInterceptor.PerformanceTests;

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

        services.AddLogging(cfg => cfg.AddConsole().AddDebug().SetMinimumLevel(LogLevel.Warning));
        services.AddEFSecondLevelCache(options => options.UseMemoryCacheProvider().ConfigureLogging(enable: true));

        var basePath = Directory.GetCurrentDirectory();
        Console.WriteLine($"Using `{basePath}` as the ContentRootPath");

        var configuration = new ConfigurationBuilder().SetBasePath(basePath)
            .AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        services.AddSingleton(_ => configuration);
        services.AddConfiguredMsSqlDbContext(getConnectionString(basePath, configuration));

        return services.BuildServiceProvider();
    }

    private static string getConnectionString(string basePath, IConfigurationRoot configuration)
    {
        var testsFolder = basePath.Split(new[]
        {
            "\\Tests\\"
        }, StringSplitOptions.RemoveEmptyEntries)[0];

        var contentRootPath = Path.Combine(testsFolder, path2: "Tests",
            path3: "EFCoreSecondLevelCacheInterceptor.AspNetCoreSample");

        var connectionString = configuration[key: "ConnectionStrings:ApplicationDbContextConnection"];

        if (connectionString is null)
        {
            throw new InvalidOperationException(message: "connectionString is null");
        }

        if (connectionString.Contains(value: "%CONTENTROOTPATH%", StringComparison.Ordinal))
        {
            connectionString =
                connectionString.Replace(oldValue: "%CONTENTROOTPATH%", contentRootPath, StringComparison.Ordinal);
        }

        Console.WriteLine($"Using {connectionString}");

        return connectionString;
    }
}