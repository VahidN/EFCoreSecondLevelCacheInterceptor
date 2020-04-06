using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer;
using System;
using Lamar;

namespace EFCoreSecondLevelCacheInterceptor.AspNetCoreSampleWithLamar
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

        // From https://jasperfx.github.io/lamar/documentation/ioc/aspnetcore/
        // Take in Lamar's ServiceRegistry instead of IServiceCollection
        // as your argument, but fear not, it implements IServiceCollection
        // as well
        public void ConfigureContainer(ServiceRegistry services)
        {
            services.AddEFSecondLevelCache(options =>
            {
                options.UseMemoryCacheProvider()
                    .CacheAllQueries(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(30));
            });

            var connectionString = Configuration["ConnectionStrings:ApplicationDbContextConnection"];
            if (connectionString.Contains("%CONTENTROOTPATH%"))
            {
                connectionString = connectionString.Replace("%CONTENTROOTPATH%", _contentRootPath);
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
