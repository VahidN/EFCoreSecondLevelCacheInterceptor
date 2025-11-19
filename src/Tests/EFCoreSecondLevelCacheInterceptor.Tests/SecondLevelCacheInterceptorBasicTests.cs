using CacheManager.Serialization.Json;
using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EFCoreSecondLevelCacheInterceptor.Tests;

[TestClass]
public class SecondLevelCacheInterceptorBasicTests
{
    [TestMethod]
    [DataRow(TestCacheProvider.BuiltInInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
    [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
    [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
    [DataRow(TestCacheProvider.StackExchangeRedis)]
    public void TestIncludeMethodAffectsKeyCache(TestCacheProvider cacheProvider)
        => EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, cacheAllQueries: false,
            (context, loggerProvider) =>
            {
                var firstProductIncludeTags = context.Products.Include(x => x.TagProducts)
                    .ThenInclude(x => x.Tag)
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .FirstOrDefault();

                Assert.IsNotNull(firstProductIncludeTags);
                Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());

                var firstProduct = context.Products
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .FirstOrDefault();

                Assert.IsNotNull(firstProduct);
                Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());
            });

    [TestMethod]
    [DataRow(TestCacheProvider.BuiltInInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
    [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
    [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
    [DataRow(TestCacheProvider.StackExchangeRedis)]
    public void TestQueriesUsingDifferentParameterValuesWillNotUseTheCache(TestCacheProvider cacheProvider)
        => EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, cacheAllQueries: false,
            (context, loggerProvider) =>
            {
                var list1 = context.Products.Include(x => x.TagProducts)
                    .ThenInclude(x => x.Tag)
                    .OrderBy(product => product.ProductNumber)
                    .Where(product => product.IsActive && product.ProductName == "Product1")
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .ToList();

                Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());

                var list2 = context.Products.Include(x => x.TagProducts)
                    .ThenInclude(x => x.Tag)
                    .OrderBy(product => product.ProductNumber)
                    .Where(product => !product.IsActive && product.ProductName == "Product1")
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .ToList();

                Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());

                var list3 = context.Products.Include(x => x.TagProducts)
                    .ThenInclude(x => x.Tag)
                    .OrderBy(product => product.ProductNumber)
                    .Where(product => !product.IsActive && product.ProductName == "Product2")
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .ToList();

                Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());

                var list4 = context.Products.Include(x => x.TagProducts)
                    .ThenInclude(x => x.Tag)
                    .OrderBy(product => product.ProductNumber)
                    .Where(product => !product.IsActive && product.ProductName == "Product2")
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .ToList();

                Assert.AreEqual(expected: 1, loggerProvider.GetCacheHitCount());
            });

    [TestMethod]
    [DataRow(TestCacheProvider.BuiltInInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
    [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
    [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
    public void TestSecondLevelCacheInTwoDifferentContextsDoesNotHitTheDatabase(TestCacheProvider cacheProvider)
    {
        var isActive = true;
        var name = "Product2";

        EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, cacheAllQueries: false, (context, loggerProvider)
            =>
        {
            var list2 = context.Products.OrderBy(product => product.ProductNumber)
                .Where(product => product.IsActive == isActive && product.ProductName == name)
                .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                .ToList();

            Assert.IsTrue(list2.Any());
            Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());
        }, (context, loggerProvider) =>
        {
            var list3 = context.Products.OrderBy(product => product.ProductNumber)
                .Where(product => product.IsActive == isActive && product.ProductName == name)
                .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                .ToList();

            Assert.IsTrue(list3.Any());
            Assert.AreEqual(expected: 1, loggerProvider.GetCacheHitCount());
        });
    }

    [TestMethod]
    [DataRow(TestCacheProvider.BuiltInInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
    [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
    [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
    public void TestSecondLevelCacheUsingDifferentSyncMethods(TestCacheProvider cacheProvider)
        => EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, cacheAllQueries: false,
            (context, loggerProvider) =>
            {
                var isActive = true;
                var name = "Product1";

                var count = context.Products.OrderBy(product => product.ProductNumber)
                    .Where(product => product.IsActive == isActive && product.ProductName == name)
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .Count();

                Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());
                Assert.IsGreaterThan(lowerBound: 0, count);

                var list1 = context.Products.OrderBy(product => product.ProductNumber)
                    .Where(product => product.IsActive == isActive && product.ProductName == name)
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .ToList();

                Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());
                Assert.IsTrue(list1.Any());

                var product1 = context.Products.OrderBy(product => product.ProductNumber)
                    .Where(product => product.IsActive == isActive && product.ProductName == name)
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .FirstOrDefault();

                Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());
                Assert.IsNotNull(product1);

                var any = context.Products.OrderBy(product => product.ProductNumber)
                    .Where(product => product.IsActive == isActive && product.ProductName == "Product2")
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .Any();

                Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());
                Assert.IsTrue(any);

                var sum = context.Products.OrderBy(product => product.ProductNumber)
                    .Where(product => product.IsActive == isActive && product.ProductName == "Product2")
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .Sum(x => x.ProductId);

                Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());
                Assert.IsGreaterThan(lowerBound: 0, sum);
            });

    [TestMethod]
    [DataRow(TestCacheProvider.BuiltInInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
    [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
    [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
    public void TestSecondLevelCacheUsingTwoCountMethods(TestCacheProvider cacheProvider)
        => EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, cacheAllQueries: false,
            (context, loggerProvider) =>
            {
                var isActive = true;
                var name = "Product3";

                var count = context.Products.OrderBy(product => product.ProductNumber)
                    .Where(product => product.IsActive == isActive && product.ProductName == name)
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .Count();

                Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount(), $"cacheProvider: {cacheProvider}");
                Assert.IsGreaterThan(lowerBound: 0, count);

                count = context.Products.OrderBy(product => product.ProductNumber)
                    .Where(product => product.IsActive == isActive && product.ProductName == name)
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .Count();

                Assert.AreEqual(expected: 1, loggerProvider.GetCacheHitCount());
                Assert.IsGreaterThan(lowerBound: 0, count);
            });

    [TestMethod]
    [DataRow(TestCacheProvider.BuiltInInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
    [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
    [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
    public void TestSecondLevelCacheUsingProjections(TestCacheProvider cacheProvider)
        => EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, cacheAllQueries: false,
            (context, loggerProvider) =>
            {
                var isActive = true;
                var name = "Product1";

                var list2 = context.Products.OrderBy(product => product.ProductNumber)
                    .Where(product => product.IsActive == isActive && product.ProductName == name)
                    .Select(x => x.ProductId)
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .ToList();

                Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());
                Assert.IsTrue(list2.Any());

                list2 = context.Products.OrderBy(product => product.ProductNumber)
                    .Where(product => product.IsActive == isActive && product.ProductName == name)
                    .Select(x => x.ProductId)
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .ToList();

                Assert.AreEqual(expected: 1, loggerProvider.GetCacheHitCount());
                Assert.IsTrue(list2.Any());
            });

    [TestMethod]
    [DataRow(TestCacheProvider.BuiltInInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
    [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
    [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
    public void TestIncludeMethodAndProjectionAffectsKeyCache(TestCacheProvider cacheProvider)
    {
        var isActive = true;
        var name = "Product1";

        EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, cacheAllQueries: false, (context, loggerProvider)
            =>
        {
            var product1IncludeTags = context.Products
                .Where(product => product.IsActive == isActive && product.ProductName == name)
                .Include(x => x.TagProducts)
                .ThenInclude(x => x.Tag)
                .Select(x => new
                {
                    Name = x.ProductName,
                    Tag = x.TagProducts.Select(y => y.Tag)
                })
                .OrderBy(x => x.Name)
                .FirstOrDefault();

            Assert.IsNotNull(product1IncludeTags);
        }, (context, loggerProvider) =>
        {
            var firstProductIncludeTags = context.Products
                .Where(product => product.IsActive == isActive && product.ProductName == name)
                .Include(x => x.TagProducts)
                .ThenInclude(x => x.Tag)
                .Select(x => new
                {
                    Name = x.ProductName,
                    Tag = x.TagProducts.Select(y => y.Tag)
                })
                .OrderBy(x => x.Name)
                .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                .FirstOrDefault();

            Assert.IsNotNull(firstProductIncludeTags);
            Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());
        }, (context, loggerProvider) =>
        {
            var firstProductIncludeTags2 = context.Products
                .Where(product => product.IsActive == isActive && product.ProductName == name)
                .Include(x => x.TagProducts)
                .ThenInclude(x => x.Tag)
                .Select(x => new
                {
                    Name = x.ProductName,
                    Tag = x.TagProducts.Select(y => y.Tag)
                })
                .OrderBy(x => x.Name)
                .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                .FirstOrDefault();

            Assert.IsNotNull(firstProductIncludeTags2);
            Assert.AreEqual(expected: 1, loggerProvider.GetCacheHitCount());
        }, (context, loggerProvider) =>
        {
            var firstProduct = context.Products
                .Where(product => product.IsActive == isActive && product.ProductName == name)
                .Select(x => new
                {
                    Name = x.ProductName,
                    Tag = x.TagProducts.Select(y => y.Tag)
                })
                .OrderBy(x => x.Name)
                .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                .FirstOrDefault();

            Assert.IsNotNull(firstProduct);
            Assert.AreEqual(expected: 1, loggerProvider.GetCacheHitCount());
        });
    }

    [TestMethod]
    [DataRow(TestCacheProvider.BuiltInInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
    [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
    [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
    public void TestNullValuesWillUseTheCache(TestCacheProvider cacheProvider)
        => EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, cacheAllQueries: false,
            (context, loggerProvider) =>
            {
                var item1 = context.Products.OrderBy(product => product.ProductNumber)
                    .Where(product => product.IsActive && product.ProductName == "Product1xx")
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .FirstOrDefault();

                Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());
                Assert.IsNull(item1);

                var item2 = context.Products.OrderBy(product => product.ProductNumber)
                    .Where(product => product.IsActive && product.ProductName == "Product1xx")
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .FirstOrDefault();

                Assert.AreEqual(expected: 1, loggerProvider.GetCacheHitCount());
                Assert.IsNull(item2);
            });

    [TestMethod]
    [DataRow(TestCacheProvider.BuiltInInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
    [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
    [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
    public void TestEqualsMethodWillUseTheCache(TestCacheProvider cacheProvider)
        => EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, cacheAllQueries: false,
            (context, loggerProvider) =>
            {
                var item1 = context.Products
                    .Where(product
                        => product.ProductId == 2 && product.ProductName.Equals("Product1", StringComparison.Ordinal))
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .FirstOrDefault();

                Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());
                Assert.IsNotNull(item1);

                var item2 = context.Products
                    .Where(product
                        => product.ProductId == 2 && product.ProductName.Equals("Product1", StringComparison.Ordinal))
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .FirstOrDefault();

                Assert.AreEqual(expected: 1, loggerProvider.GetCacheHitCount());
                Assert.IsNotNull(item2);

                var item3 = context.Products
                    .Where(product
                        => product.ProductId == 1 && product.ProductName.Equals("Product1", StringComparison.Ordinal))
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .FirstOrDefault();

                Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());
                Assert.IsNull(item3);
            });

    [TestMethod]
    [DataRow(TestCacheProvider.BuiltInInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
    [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
    [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
    public void Test2DifferentCollectionsWillNotUseTheCache(TestCacheProvider cacheProvider)
        => EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, cacheAllQueries: false,
            (context, loggerProvider) =>
            {
                var collection1 = new[]
                {
                    1, 2, 3
                };

                var item1 = context.Products.Where(product => collection1.Contains(product.ProductId))
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .FirstOrDefault();

                Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());
                Assert.IsNotNull(item1);

                var collection2 = new[]
                {
                    1, 2, 3, 4
                };

                var item2 = context.Products.Where(product => collection2.Contains(product.ProductId))
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .FirstOrDefault();

                Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());
                Assert.IsNotNull(item2);
            });

    [TestMethod]
    public void TestJsonNet()
    {
        var rnd = new Random();
        var utcNow = DateTimeOffset.UtcNow;

        var userInfo = new object?[]
        {
            $"User {rnd.Next(minValue: 1, maxValue: 100000)}", DateTime.UtcNow, null, 1000, true, 1, 'C', utcNow, 1.1M,
            1.3, 1.2f, Guid.NewGuid(), TimeSpan.FromMinutes(minutes: 1), 2, new byte[]
            {
                1, 2
            },
            1, 1, 1
        };

        var jss = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
#pragma warning disable CA2326
            TypeNameHandling = TypeNameHandling.Auto,
#pragma warning restore CA2326
            Converters =
            {
                new SpecialTypesConverter()
            }
        };

        var jcs = new JsonCacheSerializer(jss, jss); // This is how the CacheManagerCore uses the Json.NET
        var json = jcs.Serialize(userInfo);
        var userData = jcs.Deserialize(json, typeof(object[])) as object[];
        Assert.IsNotNull(userData);
        Assert.AreEqual(utcNow, userData[7]);
    }

    [TestMethod]
    [DataRow(TestCacheProvider.BuiltInInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
    [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
    [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
    public void TestUsersWithAllDataTypes(TestCacheProvider cacheProvider)
        => EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, cacheAllQueries: false,
            (context, loggerProvider) =>
            {
                var items1 = context.Users.Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .ToList();

                Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());
                Assert.IsNotNull(items1);

                var items2 = context.Users.Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .ToList();

                Assert.AreEqual(expected: 1, loggerProvider.GetCacheHitCount());
                Assert.IsNotNull(items2);
            });

    [TestMethod]
    [DataRow(TestCacheProvider.BuiltInInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
    [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
    [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
    public void TestAllDateTypes(TestCacheProvider cacheProvider)
        => EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, cacheAllQueries: false,
            (context, loggerProvider) =>
            {
                var items1 = context.DateTypes
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .ToList();

                Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());
                Assert.IsNotNull(items1);

                Assert.IsGreaterThan(lowerBound: 0, items1[index: 0].UpdateDate.Millisecond);

                var items2 = context.DateTypes
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .ToList();

                Assert.AreEqual(expected: 1, loggerProvider.GetCacheHitCount());
                Assert.IsNotNull(items2);

                Assert.IsGreaterThan(lowerBound: 0, items2[index: 0].UpdateDate.Millisecond);
            });

    [TestMethod]
    [DataRow(TestCacheProvider.BuiltInInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
    [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
    [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
    [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
    public void TestSecondLevelCache_with_additional_tags_does_not_hit_the_database(TestCacheProvider cacheProvider)
        => EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, cacheAllQueries: false,
            (context, loggerProvider) =>
            {
                var isActive = true;
                var productName = "Product2";

                var list1 = context.Products.TagWith(tag: "Custom Tag")
                    .OrderBy(product => product.ProductNumber)
                    .Where(product => product.IsActive == isActive && product.ProductName == productName)
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .ToList();

                Assert.IsTrue(list1.Any());
                Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());

                var list2 = context.Products.TagWith(tag: "Custom Tag")
                    .OrderBy(product => product.ProductNumber)
                    .Where(product => product.IsActive == isActive && product.ProductName == productName)
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                    .ToList();

                Assert.IsTrue(list2.Any());
                Assert.AreEqual(expected: 1, loggerProvider.GetCacheHitCount());
            });

    [TestMethod]
    public void TestInstantiatingContextWithoutDI()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        var basePath = Directory.GetCurrentDirectory();
        Console.WriteLine($"Using `{basePath}` as the ContentRootPath");

        var configuration = new ConfigurationBuilder().SetBasePath(basePath)
            .AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        services.AddSingleton(_ => configuration);

        using var loggerProvider = new DebugLoggerProvider();

        services.AddLogging(cfg
            => cfg.AddConsole().AddDebug().AddProvider(loggerProvider).SetMinimumLevel(LogLevel.Debug));

        services.AddEFSecondLevelCache(options
            => options.UseMemoryCacheProvider(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 50)));

        using var serviceProvider = services.BuildServiceProvider();

        using var loggerFactory = new LoggerFactory(new[]
        {
            loggerProvider
        });

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>().UseLoggerFactory(loggerFactory)
            .UseSqlServer(EFServiceProvider.GetConnectionString(basePath, configuration), sqlServerOptionsBuilder =>
            {
                sqlServerOptionsBuilder.CommandTimeout((int)TimeSpan.FromMinutes(minutes: 3).TotalSeconds);
                sqlServerOptionsBuilder.MigrationsAssembly(typeof(MsSqlServiceCollectionExtensions).Assembly.FullName);
            })
            .AddInterceptors(serviceProvider.GetRequiredService<SecondLevelCacheInterceptor>());

        var options = optionsBuilder.Options;

        using (var context = new ApplicationDbContext(options))
        {
            var items1 = context.DateTypes.Cacheable().ToList();
            Assert.AreEqual(expected: 0, loggerProvider.GetCacheHitCount());
            Assert.IsNotNull(items1);

            var items2 = context.DateTypes.Cacheable().ToList();
            Assert.AreEqual(expected: 1, loggerProvider.GetCacheHitCount());
            Assert.IsNotNull(items2);
        }
    }
}