using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EFCoreSecondLevelCacheInterceptor.Tests
{
    [TestClass]
    public class SecondLevelCacheInterceptorBasicTests
    {
        [DataTestMethod]
        [DataRow(TestCacheProvider.BuiltInInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
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
        public void TestIncludeMethodAndProjectionAffectsKeyCache(TestCacheProvider cacheProvider)
        {
            EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, false,
                (context, loggerProvider) =>
                {
                    var product1IncludeTags = context.Products
                        .Include(x => x.TagProducts).ThenInclude(x => x.Tag)
                        .Select(x => new { Name = x.ProductName, Tag = x.TagProducts.Select(y => y.Tag) })
                        .OrderBy(x => x.Name)
                        .FirstOrDefault();
                    Assert.IsNotNull(product1IncludeTags);
                },
                (context, loggerProvider) =>
                {
                    var firstProductIncludeTags = context.Products
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
    }
}