using System;
using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer;
using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EFCoreSecondLevelCacheInterceptor.AspNetCoreSample;

public class Startup
{
    private readonly string _contentRootPath;
    private readonly IWebHostEnvironment _env;

    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
        _env = env;
        _contentRootPath = env.ContentRootPath;
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
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
                .ConfigureLogging(_env.IsDevelopment(), args =>
                {
                    switch (args.EventId)
                    {
                        case CacheableLogEventId.None:
                            break;
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
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                })

                //.CacheAllQueries(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(30)
                /*.CacheQueriesContainingTypes(
                    CacheExpirationMode.Absolute, TimeSpan.FromMinutes(30),
                    typeof(Post), typeof(Product), typeof(User)
                    )*/
                .CacheQueriesContainingTableNames(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(value: 30),
                    TableNameComparison.ContainsOnly, "posts", "products", "users")
                .SkipCachingCommands(commandText =>

                    // How to skip caching specific commands
                    commandText.Contains(value: "NEWID()", StringComparison.InvariantCultureIgnoreCase))

                // Don't cache null values. Remove this optional setting if it's not necessary.
                .SkipCachingResults(result
                    => result.Value == null || (result.Value is EFTableRows rows && rows.RowsCount == 0))
                .SkipCacheInvalidationCommands(commandText =>

                    // How to skip invalidating the related cache entries of this query
                    commandText.Contains(value: "NEWID()", StringComparison.InvariantCultureIgnoreCase))
                .OverrideCachePolicy(context =>
                {
                    if (context.IsCrudCommand)
                    {
                        return null;
                    }

                    if (context.CommandTableNames.Contains(item: "posts"))
                    {
                        return new EFCachePolicy().ExpirationMode(CacheExpirationMode.NeverRemove);
                    }

                    return null; // Use the default/calculated EFCachePolicy 
                });
        });

        var connectionString = Configuration[key: "ConnectionStrings:ApplicationDbContextConnection"];

        if (connectionString.Contains(value: "%CONTENTROOTPATH%"))
        {
            connectionString = connectionString.Replace(oldValue: "%CONTENTROOTPATH%", _contentRootPath);
        }

        services.AddConfiguredMsSqlDbContext(connectionString);

        services.AddControllersWithViews();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceScopeFactory scopeFactory)
    {
        scopeFactory.Initialize();
        scopeFactory.SeedData();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler(errorHandlingPath: "/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
        });
    }
}