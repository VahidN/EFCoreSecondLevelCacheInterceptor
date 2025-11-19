using Lamar.Microsoft.DependencyInjection;

namespace EFCoreSecondLevelCacheInterceptor.AspNetCoreSampleWithLamar;

public static class Program
{
    public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

    public static IHostBuilder CreateHostBuilder(string[] args)
        => Host.CreateDefaultBuilder(args)

            // Add Lamar
            .UseLamar()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureLogging((hostingContext, logging) =>
                    {
                        logging.ClearProviders();
                        logging.AddConfiguration(hostingContext.Configuration.GetSection(key: "Logging"));
                        logging.AddDebug();

                        if (hostingContext.HostingEnvironment.IsDevelopment())
                        {
                            logging.AddConsole();
                        }
                    })
                    .UseStartup<Startup>();
            });
}