using System;
using System.IO;
using System.Linq;
using CacheManager.Serialization.Json;
using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer;
using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace EFCoreSecondLevelCacheInterceptor.Tests
{
    [TestClass]
    public class SecondLevelCacheInterceptorBasicTests
    {
        [DataTestMethod]
        [DataRow(TestCacheProvider.BuiltInInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
        [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
        [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
        public void TestIncludeMethodAffectsKeyCache(TestCacheProvider cacheProvider)
        {
            EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, false, (context, loggerProvider) =>
               {
                   var firstProductIncludeTags = context.Products.Include(x => x.TagProducts).ThenInclude(x => x.Tag)
                                                               .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                                                               .FirstOrDefault();
                   Assert.IsNotNull(firstProductIncludeTags);
                   Assert.AreEqual(0, loggerProvider.GetCacheHitCount());

                   var firstProduct = context.Products.Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45)).FirstOrDefault();
                   Assert.IsNotNull(firstProduct);
                   Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
               });
        }

        [DataTestMethod]
        [DataRow(TestCacheProvider.BuiltInInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
        [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
        [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
        public void TestQueriesUsingDifferentParameterValuesWillNotUseTheCache(TestCacheProvider cacheProvider)
        {
            EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, false, (context, loggerProvider) =>
               {
                   var list1 = context.Products.Include(x => x.TagProducts).ThenInclude(x => x.Tag)
                       .OrderBy(product => product.ProductNumber)
                       .Where(product => product.IsActive && product.ProductName == "Product1")
                       .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                       .ToList();
                   Assert.AreEqual(0, loggerProvider.GetCacheHitCount());

                   var list2 = context.Products.Include(x => x.TagProducts).ThenInclude(x => x.Tag)
                       .OrderBy(product => product.ProductNumber)
                       .Where(product => !product.IsActive && product.ProductName == "Product1")
                       .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                       .ToList();
                   Assert.AreEqual(0, loggerProvider.GetCacheHitCount());

                   var list3 = context.Products.Include(x => x.TagProducts).ThenInclude(x => x.Tag)
                       .OrderBy(product => product.ProductNumber)
                       .Where(product => !product.IsActive && product.ProductName == "Product2")
                       .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                       .ToList();
                   Assert.AreEqual(0, loggerProvider.GetCacheHitCount());

                   var list4 = context.Products.Include(x => x.TagProducts).ThenInclude(x => x.Tag)
                       .OrderBy(product => product.ProductNumber)
                       .Where(product => !product.IsActive && product.ProductName == "Product2")
                       .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                       .ToList();
                   Assert.AreEqual(1, loggerProvider.GetCacheHitCount());
               });
        }

        [DataTestMethod]
        [DataRow(TestCacheProvider.BuiltInInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
        [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
        [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
        public void TestSecondLevelCacheInTwoDifferentContextsDoesNotHitTheDatabase(TestCacheProvider cacheProvider)
        {
            var isActive = true;
            var name = "Product2";

            EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, false,
                (context, loggerProvider) =>
                {
                    var list2 = context.Products
                        .OrderBy(product => product.ProductNumber)
                        .Where(product => product.IsActive == isActive && product.ProductName == name)
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .ToList();
                    Assert.IsTrue(list2.Any());
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                },
                (context, loggerProvider) =>
                {
                    var list3 = context.Products
                                    .OrderBy(product => product.ProductNumber)
                                    .Where(product => product.IsActive == isActive && product.ProductName == name)
                                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                                    .ToList();
                    Assert.IsTrue(list3.Any());
                    Assert.AreEqual(1, loggerProvider.GetCacheHitCount());
                });
        }

        [DataTestMethod]
        [DataRow(TestCacheProvider.BuiltInInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
        [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
        [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
        public void TestSecondLevelCacheUsingDifferentSyncMethods(TestCacheProvider cacheProvider)
        {
            EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, false,
                (context, loggerProvider) =>
                {
                    var isActive = true;
                    var name = "Product1";

                    var count = context.Products
                                    .OrderBy(product => product.ProductNumber)
                                    .Where(product => product.IsActive == isActive && product.ProductName == name)
                                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                                    .Count();
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                    Assert.IsTrue(count > 0);

                    var list1 = context.Products
                                    .OrderBy(product => product.ProductNumber)
                                    .Where(product => product.IsActive == isActive && product.ProductName == name)
                                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                                    .ToList();
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                    Assert.IsTrue(list1.Any());

                    var product1 = context.Products
                                    .OrderBy(product => product.ProductNumber)
                                    .Where(product => product.IsActive == isActive && product.ProductName == name)
                                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                                    .FirstOrDefault();
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                    Assert.IsTrue(product1 != null);

                    var any = context.Products
                                    .OrderBy(product => product.ProductNumber)
                                    .Where(product => product.IsActive == isActive && product.ProductName == "Product2")
                                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                                    .Any();
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                    Assert.IsTrue(any);

                    var sum = context.Products
                        .OrderBy(product => product.ProductNumber)
                        .Where(product => product.IsActive == isActive && product.ProductName == "Product2")
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .Sum(x => x.ProductId);
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                    Assert.IsTrue(sum > 0);
                });
        }


        [DataTestMethod]
        [DataRow(TestCacheProvider.BuiltInInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
        [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
        [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
        public void TestSecondLevelCacheUsingTwoCountMethods(TestCacheProvider cacheProvider)
        {
            EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, false, (context, loggerProvider) =>
               {
                   var isActive = true;
                   var name = "Product3";

                   var count = context.Products
                                   .OrderBy(product => product.ProductNumber)
                                   .Where(product => product.IsActive == isActive && product.ProductName == name)
                                   .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                                   .Count();
                   Assert.AreEqual(0, loggerProvider.GetCacheHitCount(), $"cacheProvider: {cacheProvider}");
                   Assert.IsTrue(count > 0);

                   count = context.Products
                                   .OrderBy(product => product.ProductNumber)
                                   .Where(product => product.IsActive == isActive && product.ProductName == name)
                                   .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                                   .Count();
                   Assert.AreEqual(1, loggerProvider.GetCacheHitCount());
                   Assert.IsTrue(count > 0);
               });
        }

        [DataTestMethod]
        [DataRow(TestCacheProvider.BuiltInInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
        [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
        [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
        public void TestSecondLevelCacheUsingProjections(TestCacheProvider cacheProvider)
        {
            EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, false, (context, loggerProvider) =>
               {
                   var isActive = true;
                   var name = "Product1";

                   var list2 = context.Products
                                   .OrderBy(product => product.ProductNumber)
                                   .Where(product => product.IsActive == isActive && product.ProductName == name)
                                   .Select(x => x.ProductId)
                                   .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                                   .ToList();
                   Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                   Assert.IsTrue(list2.Any());

                   list2 = context.Products
                                   .OrderBy(product => product.ProductNumber)
                                   .Where(product => product.IsActive == isActive && product.ProductName == name)
                                   .Select(x => x.ProductId)
                                   .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                                   .ToList();
                   Assert.AreEqual(1, loggerProvider.GetCacheHitCount());
                   Assert.IsTrue(list2.Any());
               });
        }

        [DataTestMethod]
        [DataRow(TestCacheProvider.BuiltInInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
        [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
        [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
        public void TestIncludeMethodAndProjectionAffectsKeyCache(TestCacheProvider cacheProvider)
        {
            var isActive = true;
            var name = "Product1";

            EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, false,
                (context, loggerProvider) =>
                {
                    var product1IncludeTags = context.Products
                        .Where(product => product.IsActive == isActive && product.ProductName == name)
                        .Include(x => x.TagProducts).ThenInclude(x => x.Tag)
                        .Select(x => new { Name = x.ProductName, Tag = x.TagProducts.Select(y => y.Tag) })
                        .OrderBy(x => x.Name)
                        .FirstOrDefault();
                    Assert.IsNotNull(product1IncludeTags);
                },
                (context, loggerProvider) =>
                {
                    var firstProductIncludeTags = context.Products
                        .Where(product => product.IsActive == isActive && product.ProductName == name)
                        .Include(x => x.TagProducts).ThenInclude(x => x.Tag)
                        .Select(x => new { Name = x.ProductName, Tag = x.TagProducts.Select(y => y.Tag) })
                        .OrderBy(x => x.Name)
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .FirstOrDefault();
                    Assert.IsNotNull(firstProductIncludeTags);
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                },
                (context, loggerProvider) =>
                {
                    var firstProductIncludeTags2 = context.Products
                        .Where(product => product.IsActive == isActive && product.ProductName == name)
                        .Include(x => x.TagProducts).ThenInclude(x => x.Tag)
                        .Select(x => new { Name = x.ProductName, Tag = x.TagProducts.Select(y => y.Tag) })
                        .OrderBy(x => x.Name)
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .FirstOrDefault();
                    Assert.IsNotNull(firstProductIncludeTags2);
                    Assert.AreEqual(1, loggerProvider.GetCacheHitCount());
                },
                (context, loggerProvider) =>
                {
                    var firstProduct = context.Products
                        .Where(product => product.IsActive == isActive && product.ProductName == name)
                        .Select(x => new { Name = x.ProductName, Tag = x.TagProducts.Select(y => y.Tag) })
                        .OrderBy(x => x.Name)
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .FirstOrDefault();
                    Assert.IsNotNull(firstProduct);
                    Assert.AreEqual(1, loggerProvider.GetCacheHitCount());
                });
        }

        [DataTestMethod]
        [DataRow(TestCacheProvider.BuiltInInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
        [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
        [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
        public void TestNullValuesWillUseTheCache(TestCacheProvider cacheProvider)
        {
            EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, false, (context, loggerProvider) =>
               {
                   var item1 = context.Products
                       .OrderBy(product => product.ProductNumber)
                       .Where(product => product.IsActive && product.ProductName == "Product1xx")
                       .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                       .FirstOrDefault();
                   Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                   Assert.IsNull(item1);

                   var item2 = context.Products
                       .OrderBy(product => product.ProductNumber)
                       .Where(product => product.IsActive && product.ProductName == "Product1xx")
                       .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                       .FirstOrDefault();
                   Assert.AreEqual(1, loggerProvider.GetCacheHitCount());
                   Assert.IsNull(item2);
               });
        }

        [DataTestMethod]
        [DataRow(TestCacheProvider.BuiltInInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
        [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
        [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
        public void TestEqualsMethodWillUseTheCache(TestCacheProvider cacheProvider)
        {
            EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, false, (context, loggerProvider) =>
               {
                   var item1 = context.Products
                       .Where(product => product.ProductId == 2 && product.ProductName.Equals("Product1"))
                       .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                       .FirstOrDefault();
                   Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                   Assert.IsNotNull(item1);

                   var item2 = context.Products
                       .Where(product => product.ProductId == 2 && product.ProductName.Equals("Product1"))
                       .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                       .FirstOrDefault();
                   Assert.AreEqual(1, loggerProvider.GetCacheHitCount());
                   Assert.IsNotNull(item2);

                   var item3 = context.Products
                       .Where(product => product.ProductId == 1 && product.ProductName.Equals("Product1"))
                       .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                       .FirstOrDefault();
                   Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                   Assert.IsNull(item3);
               });
        }

        [DataTestMethod]
        [DataRow(TestCacheProvider.BuiltInInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
        [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
        [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
        public void Test2DifferentCollectionsWillNotUseTheCache(TestCacheProvider cacheProvider)
        {
            EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, false,
                (context, loggerProvider) =>
                {
                    var collection1 = new[] { 1, 2, 3 };

                    var item1 = context.Products
                        .Where(product => collection1.Contains(product.ProductId))
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .FirstOrDefault();
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                    Assert.IsNotNull(item1);

                    var collection2 = new[] { 1, 2, 3, 4 };

                    var item2 = context.Products
                        .Where(product => collection2.Contains(product.ProductId))
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .FirstOrDefault();
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                    Assert.IsNotNull(item2);
                });
        }

        [TestMethod]
        public void TestJsonNet()
        {
            var rnd = new Random();
            var utcNow = DateTimeOffset.UtcNow;
            var userInfo = new object[]  // EFTableRow -> public object[] Values { get; set; }
            {
                $"User {rnd.Next(1, 100000)}",
                DateTime.UtcNow,
                null,
                1000,
                true,
                1,
                'C',
                utcNow,
                1.1M,
                1.3,
                1.2f,
                Guid.NewGuid(),
                TimeSpan.FromMinutes(1),
                2,
                new byte[] { 1, 2 },
                1,
                1,
                1
            };

            var jss = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
                Converters = { new SpecialTypesConverter() }
            };
            var jcs = new JsonCacheSerializer(jss, jss); // This is how the CacheManagerCore uses the Json.NET
            var json = jcs.Serialize(userInfo);
            var userData = jcs.Deserialize(json, typeof(object[])) as object[];
            Assert.IsNotNull(userData);
            Assert.AreEqual(utcNow, userData[7]);
        }

        [DataTestMethod]
        [DataRow(TestCacheProvider.BuiltInInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
        [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
        [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
        public void TestUsersWithAllDataTypes(TestCacheProvider cacheProvider)
        {
            EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, false,
                (context, loggerProvider) =>
                {
                    var items1 = context.Users
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .ToList();
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                    Assert.IsNotNull(items1);

                    var items2 = context.Users
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .ToList();
                    Assert.AreEqual(1, loggerProvider.GetCacheHitCount());
                    Assert.IsNotNull(items2);
                });
        }

        [DataTestMethod]
        [DataRow(TestCacheProvider.BuiltInInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
        [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
        [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
        public void TestAllDateTypes(TestCacheProvider cacheProvider)
        {
            EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, false,
                (context, loggerProvider) =>
                {
                    var items1 = context.DateTypes
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .ToList();
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                    Assert.IsNotNull(items1);

                    Assert.IsTrue(items1.First().UpdateDate.Millisecond > 0);

                    var items2 = context.DateTypes
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .ToList();
                    Assert.AreEqual(1, loggerProvider.GetCacheHitCount());
                    Assert.IsNotNull(items2);

                    Assert.IsTrue(items2.First().UpdateDate.Millisecond > 0);
                });
        }

        [DataTestMethod]
        [DataRow(TestCacheProvider.BuiltInInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
        [DataRow(TestCacheProvider.EasyCachingCoreInMemory)]
        [DataRow(TestCacheProvider.EasyCachingCoreRedis)]
        public void TestSecondLevelCache_with_additional_tags_does_not_hit_the_database(TestCacheProvider cacheProvider)
        {
            EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, false,
                (context, loggerProvider) =>
                {
                    var isActive = true;
                    var productName = "Product2";

                    var list1 = context.Products
                        .TagWith("Custom Tag")
                        .OrderBy(product => product.ProductNumber)
                        .Where(product => product.IsActive == isActive && product.ProductName == productName)
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .ToList();
                    Assert.IsTrue(list1.Any());
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());

                    var list2 = context.Products
                        .TagWith("Custom Tag")
                        .OrderBy(product => product.ProductNumber)
                        .Where(product => product.IsActive == isActive && product.ProductName == productName)
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .ToList();

                    Assert.IsTrue(list2.Any());
                    Assert.AreEqual(1, loggerProvider.GetCacheHitCount());
                });
        }

        [DataTestMethod]
        public void TestInstantiatingContextWithoutDI()
        {
            var services = new ServiceCollection();
            services.AddOptions();
            var basePath = Directory.GetCurrentDirectory();
            Console.WriteLine($"Using `{basePath}` as the ContentRootPath");
            var configuration = new ConfigurationBuilder()
                                .SetBasePath(basePath)
                                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                .Build();
            services.AddSingleton(_ => configuration);

            var loggerProvider = new DebugLoggerProvider();
            services.AddLogging(cfg => cfg.AddConsole().AddDebug().AddProvider(loggerProvider).SetMinimumLevel(LogLevel.Debug));

            services.AddEFSecondLevelCache(options =>
                    options.UseMemoryCacheProvider(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(50))
            );
            var serviceProvider = services.BuildServiceProvider();

            var loggerFactory = new LoggerFactory(new[] { loggerProvider });
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(
                        EFServiceProvider.GetConnectionString(basePath, configuration),
                        sqlServerOptionsBuilder =>
                        {
                            sqlServerOptionsBuilder.CommandTimeout((int)TimeSpan.FromMinutes(3).TotalSeconds);
                            sqlServerOptionsBuilder.EnableRetryOnFailure();
                            sqlServerOptionsBuilder.MigrationsAssembly(typeof(MsSqlServiceCollectionExtensions).Assembly.FullName);
                        })
                    .AddInterceptors(serviceProvider.GetRequiredService<SecondLevelCacheInterceptor>());
            var options = (DbContextOptions<ApplicationDbContext>)optionsBuilder.Options;

            using (var context = new ApplicationDbContext(options))
            {
                var items1 = context.DateTypes
                    .Cacheable()
                    .ToList();
                Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                Assert.IsNotNull(items1);

                var items2 = context.DateTypes
                    .Cacheable()
                    .ToList();
                Assert.AreEqual(1, loggerProvider.GetCacheHitCount());
                Assert.IsNotNull(items2);
            }
        }
    }
}