using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EFCoreSecondLevelCacheInterceptor.Tests.DataLayer;

public class MsSqlContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var services = new ServiceCollection();

        services.AddLogging(cfg => cfg.AddConsole().AddDebug());

        var basePath = Directory.GetCurrentDirectory();
        Console.WriteLine($"Using `{basePath}` as the ContentRootPath");

        var configuration = new ConfigurationBuilder().SetBasePath(basePath)
            .AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        services.AddSingleton(_ => configuration);

        var connectionString = configuration[key: "ConnectionStrings:ApplicationDbContextConnection"];

        if (connectionString is null)
        {
            throw new InvalidOperationException(message: "connectionString is null");
        }

        if (connectionString.Contains(value: "%CONTENTROOTPATH%", StringComparison.InvariantCulture))
        {
            connectionString = connectionString.Replace(oldValue: "%CONTENTROOTPATH%", basePath,
                StringComparison.InvariantCulture);
        }

        services.AddEFSecondLevelCache(options
            => options.UseMemoryCacheProvider(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 5)));

        services.AddConfiguredMsSqlDbContext(connectionString);

        using var buildServiceProvider = services.BuildServiceProvider();

        return buildServiceProvider.GetRequiredService<ApplicationDbContext>();
    }
}