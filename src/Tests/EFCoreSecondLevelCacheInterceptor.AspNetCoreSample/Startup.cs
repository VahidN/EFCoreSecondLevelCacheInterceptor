using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer;
using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Entities;
using System;

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
            {
                options.UseMemoryCacheProvider(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(30))
                    .DisableLogging(false)
                    //.CacheAllQueries(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(30)
                    /*.CacheQueriesContainingTypes(
                        CacheExpirationMode.Absolute, TimeSpan.FromMinutes(30),
                        typeof(Post), typeof(Product), typeof(User)
                        )*/
                    .CacheQueriesContainingTableNames(
                        CacheExpirationMode.Absolute, TimeSpan.FromMinutes(30),
                        "posts", "products", "users"
                        )
                    .SkipCachingCommands(commandText =>
                                // How to skip caching specific commands
                                commandText.Contains("NEWID()", StringComparison.InvariantCultureIgnoreCase))
                    // Don't cache null values. Remove this optional setting if it's not necessary.
                    .SkipCachingResults(result =>
                                result.Value == null || (result.Value is EFTableRows rows && rows.RowsCount == 0));
            });

            var connectionString = Configuration["ConnectionStrings:ApplicationDbContextConnection"];
            if (connectionString.Contains("%CONTENTROOTPATH%"))
            {
                connectionString = connectionString.Replace("%CONTENTROOTPATH%", _contentRootPath);
            }
            services.AddConfiguredMsSqlDbContext(connectionString);

            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            IServiceScopeFactory scopeFactory)
        {
            scopeFactory.Initialize();
            scopeFactory.SeedData();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}