using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer;
using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Utils;
using Lamar;

namespace EFCoreSecondLevelCacheInterceptor.AspNetCoreSampleWithLamar;

public class Startup(IConfiguration configuration, IWebHostEnvironment env)
{
    private readonly string _contentRootPath = env.ContentRootPath;

    public IConfiguration Configuration { get; } = configuration;

    // From https://jasperfx.github.io/lamar/documentation/ioc/aspnetcore/
    // Take in Lamar's ServiceRegistry instead of IServiceCollection
    // as your argument, but fear not, it implements IServiceCollection
    // as well
    public void ConfigureContainer(ServiceRegistry services)
    {
        services.AddEFSecondLevelCache(options =>
        {
            options.UseMemoryCacheProvider()
                .CacheAllQueries(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 30));
        });

        var connectionString = Configuration[key: "ConnectionStrings:ApplicationDbContextConnection"];

        if (connectionString is null)
        {
            throw new InvalidOperationException(message: "connectionString is null");
        }

        if (connectionString.Contains(value: "%CONTENTROOTPATH%", StringComparison.Ordinal))
        {
            connectionString = connectionString.Replace(oldValue: "%CONTENTROOTPATH%", _contentRootPath,
                StringComparison.Ordinal);
        }

        services.AddConfiguredMsSqlDbContext(connectionString);

        services.AddControllersWithViews();

        // Also exposes Lamar specific registrations
        // and functionality
        services.Scan(s =>
        {
            s.TheCallingAssembly();
            s.WithDefaultConventions();
        });
    }

#pragma warning disable S2325
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceScopeFactory scopeFactory)
#pragma warning restore S2325
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