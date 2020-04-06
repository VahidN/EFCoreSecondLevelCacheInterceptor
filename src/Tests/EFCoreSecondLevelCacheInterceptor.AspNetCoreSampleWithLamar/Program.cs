using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EFCoreSecondLevelCacheInterceptor.AspNetCoreSampleWithLamar
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                // Add Lamar
                .UseLamar()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                            .ConfigureLogging((hostingContext, logging) =>
                                    {
                                        logging.ClearProviders();
                                        logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                                        logging.AddDebug();

                                        if (hostingContext.HostingEnvironment.IsDevelopment())
                                        {
                                            logging.AddConsole();
                                        }
                                    })
                            .UseStartup<Startup>();
                });
    }
}