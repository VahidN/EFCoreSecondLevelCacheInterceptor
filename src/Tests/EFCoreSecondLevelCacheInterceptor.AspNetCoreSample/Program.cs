namespace EFCoreSecondLevelCacheInterceptor.AspNetCoreSample;

public static class Program
{
    public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

    public static IHostBuilder CreateHostBuilder(string[] args)
        => Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureLogging((hostingContext, logging) =>
                    {
                        logging.ClearProviders();

                        logging.AddDebug();

                        if (hostingContext.HostingEnvironment.IsDevelopment())
                        {
                            logging.AddConsole();
                        }

                        logging.AddConfiguration(hostingContext.Configuration.GetSection(key: "Logging"));
                    })
                    .UseStartup<Startup>();
            });
}