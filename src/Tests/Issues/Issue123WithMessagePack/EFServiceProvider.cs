using System;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using EFCoreSecondLevelCacheInterceptor;
using Microsoft.EntityFrameworkCore;
using Issue123WithMessagePack.DataLayer;
using MessagePack.Resolvers;
using MessagePack.Formatters;
using MessagePack;

namespace Issue123WithMessagePack
{
    public static class EFServiceProvider
    {
        private static readonly Lazy<IServiceProvider> _serviceProviderBuilder =
                new Lazy<IServiceProvider>(getServiceProvider, LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// A lazy loaded thread-safe singleton
        /// </summary>
        public static IServiceProvider Instance { get; } = _serviceProviderBuilder.Value;

        public static T GetRequiredService<T>()
        {
            return Instance.GetRequiredService<T>();
        }

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
                    cfg.EnableLogging = true;
                    cfg.SerializerName = "Pack";
                    cfg.DBConfig.Endpoints.Add(new EasyCaching.Core.Configurations.ServerEndPoint("127.0.0.1", 6379));
                    cfg.DBConfig.AllowAdmin = true;
                    cfg.DBConfig.ConnectionTimeout = 10000;
                }, providerName);
                o.WithMessagePack(so =>
                    {
                        so.EnableCustomResolver = true;
                        so.CustomResolvers = CompositeResolver.Create(
                        new IMessagePackFormatter[]
                        {
                            DBNullFormatter.Instance // This is necessary for the null values
                        },
                        new IFormatterResolver[]
                        {
                            NativeDateTimeResolver.Instance,
                            ContractlessStandardResolver.Instance,
                            StandardResolverAllowPrivate.Instance,
                            TypelessContractlessStandardResolver.Instance,
                        });
                    },
                    "Pack");
            });
            services.AddEFSecondLevelCache(o =>
            {
                o.UseEasyCachingCoreProvider(providerName, isHybridCache: false).DisableLogging(false);
                o.CacheAllQueries(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(10));
                o.UseCacheKeyPrefix("EF_");
            });

            var basePath = Directory.GetCurrentDirectory();
            Console.WriteLine($"Using `{basePath}` as the ContentRootPath");
            var configuration = new ConfigurationBuilder()
                                .SetBasePath(basePath)
                                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                .Build();
            services.AddSingleton(_ => configuration);
            services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
                {
                    options
                        .AddInterceptors(serviceProvider.GetRequiredService<SecondLevelCacheInterceptor>())
                        .UseSqlServer(GetConnectionString(basePath, configuration))
                        .LogTo(sql => Console.WriteLine(sql));
                });

            return services.BuildServiceProvider();
        }

        public static string GetConnectionString(string basePath, IConfigurationRoot configuration)
        {
            var testsFolder = basePath.Split(new[] { "\\Issues\\" }, StringSplitOptions.RemoveEmptyEntries)[0];
            var contentRootPath = Path.Combine(testsFolder, "Issues", "Issue123WithMessagePack");
            var connectionString = configuration["ConnectionStrings:ApplicationDbContextConnection"];
            if (connectionString.Contains("%CONTENTROOTPATH%"))
            {
                connectionString = connectionString.Replace("%CONTENTROOTPATH%", contentRootPath);
            }
            Console.WriteLine($"Using {connectionString}");
            return connectionString;
        }
    }

    public class DBNullFormatter : IMessagePackFormatter<DBNull>
    {
        public static DBNullFormatter Instance = new();

        private DBNullFormatter()
        {
        }

        public void Serialize(ref MessagePackWriter writer, DBNull value, MessagePackSerializerOptions options)
        {
            writer.WriteNil();
        }

        public DBNull Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            return DBNull.Value;
        }
    }
}