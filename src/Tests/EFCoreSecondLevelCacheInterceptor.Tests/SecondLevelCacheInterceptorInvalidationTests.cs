using System;
using System.Linq;
using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EFCoreSecondLevelCacheInterceptor.Tests
{
    [TestClass]
    public class SecondLevelCacheInterceptorInvalidationTests
    {
        [DataTestMethod]
        [DataRow(TestCacheProvider.BuiltInInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
        public void TestInsertingDataIntoTheSameTableShouldInvalidateTheCacheAutomatically(TestCacheProvider cacheProvider)
        {
            EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, false,
                (context, loggerProvider) =>
                {
                    var isActive = true;
                    var name = "Product2";

                    var list1 = context.Products.Include(x => x.TagProducts).ThenInclude(x => x.Tag)
                        .OrderBy(product => product.ProductNumber)
                        .Where(product => product.IsActive == isActive && product.ProductName == name)
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .ToList();
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                    Assert.IsTrue(list1.Any());

                    var list2 = context.Products.Include(x => x.TagProducts).ThenInclude(x => x.Tag)
                        .OrderBy(product => product.ProductNumber)
                        .Where(product => product.IsActive == isActive && product.ProductName == name)
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .ToList();
                    Assert.AreEqual(1, loggerProvider.GetCacheHitCount());
                    Assert.IsTrue(list2.Any());

                    var newProduct = new Product
                    {
                        IsActive = false,
                        ProductName = $"Product{RandomNumberProvider.Next()}",
                        ProductNumber = RandomNumberProvider.Next().ToString(),
                        Notes = "Notes ...",
                        UserId = 1
                    };
                    context.Products.Add(newProduct);
                    context.SaveChanges();

                    var list3 = context.Products.Include(x => x.TagProducts).ThenInclude(x => x.Tag)
                        .OrderBy(product => product.ProductNumber)
                        .Where(product => product.IsActive == isActive && product.ProductName == name)
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .ToList();
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                    Assert.IsTrue(list3.Any());
                });
        }

        [DataTestMethod]
        [DataRow(TestCacheProvider.BuiltInInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
        public void TestInsertingDataToOtherTablesShouldNotInvalidateTheCacheDependencyAutomatically(TestCacheProvider cacheProvider)
        {
            EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, false, (context, loggerProvider) =>
               {
                   var isActive = true;
                   var name = "Product4";

                   var list1 = context.Products.Include(x => x.TagProducts).ThenInclude(x => x.Tag)
                       .OrderBy(product => product.ProductNumber)
                       .Where(product => product.IsActive == isActive && product.ProductName == name)
                       .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                       .ToList();
                   Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                   Assert.IsTrue(!list1.Any());

                   var list2 = context.Products.Include(x => x.TagProducts).ThenInclude(x => x.Tag)
                       .OrderBy(product => product.ProductNumber)
                       .Where(product => product.IsActive == isActive && product.ProductName == name)
                       .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                       .ToList();
                   Assert.AreEqual(1, loggerProvider.GetCacheHitCount());
                   Assert.IsTrue(!list2.Any());

                   var user = new User
                   {
                       Name = $"User {RandomNumberProvider.Next()}"
                   };
                   context.Users.Add(user);
                   context.SaveChanges();

                   var list3 = context.Products.Include(x => x.TagProducts).ThenInclude(x => x.Tag)
                       .OrderBy(product => product.ProductNumber)
                       .Where(product => product.IsActive == isActive && product.ProductName == name)
                       .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                       .ToList();
                   Assert.AreEqual(1, loggerProvider.GetCacheHitCount());
                   Assert.IsTrue(!list3.Any());
               });
        }

        [DataTestMethod]
        [DataRow(TestCacheProvider.BuiltInInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
        public void TestInsertingDataToRelatedTablesShouldInvalidateTheCacheDependencyAutomatically(TestCacheProvider cacheProvider)
        {
            EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, false,
                (context, loggerProvider) =>
                {
                    var isActive = true;
                    var name = "Product1";

                    var list1 = context.Products.Include(x => x.TagProducts).ThenInclude(x => x.Tag)
                        .OrderBy(product => product.ProductNumber)
                        .Where(product => product.IsActive == isActive && product.ProductName == name)
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .ToList();
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                    Assert.IsTrue(list1.Any());

                    var list2 = context.Products.Include(x => x.TagProducts).ThenInclude(x => x.Tag)
                        .OrderBy(product => product.ProductNumber)
                        .Where(product => product.IsActive == isActive && product.ProductName == name)
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .ToList();
                    Assert.AreEqual(1, loggerProvider.GetCacheHitCount());
                    Assert.IsTrue(list2.Any());

                    var tag = new Tag
                    {
                        Name = $"Tag {RandomNumberProvider.Next()}"
                    };
                    context.Tags.Add(tag);
                    context.SaveChanges();

                    var list3 = context.Products.Include(x => x.TagProducts).ThenInclude(x => x.Tag)
                        .OrderBy(product => product.ProductNumber)
                        .Where(product => product.IsActive == isActive && product.ProductName == name)
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .ToList();
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                    Assert.IsTrue(list3.Any());
                });
        }

        [DataTestMethod]
        [DataRow(TestCacheProvider.BuiltInInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
        public void TestTransactionRollbackShouldNotInvalidateTheCacheDependencyAutomatically(TestCacheProvider cacheProvider)
        {
            EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, false,
                (context, loggerProvider) =>
                {
                    var isActive = true;
                    var name = "Product1";

                    var list1 = context.Products.Include(x => x.TagProducts).ThenInclude(x => x.Tag)
                        .OrderBy(product => product.ProductNumber)
                        .Where(product => product.IsActive == isActive && product.ProductName == name)
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .ToList();
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                    Assert.IsTrue(list1.Any());

                    var list2 = context.Products.Include(x => x.TagProducts).ThenInclude(x => x.Tag)
                        .OrderBy(product => product.ProductNumber)
                        .Where(product => product.IsActive == isActive && product.ProductName == name)
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .ToList();
                    Assert.AreEqual(1, loggerProvider.GetCacheHitCount());
                    Assert.IsTrue(list2.Any());

                    try
                    {
                        var newProduct = new Product
                        {
                            IsActive = false,
                            ProductName = "Product1", // It has an `IsUnique` constraint.
                            ProductNumber = RandomNumberProvider.Next().ToString(),
                            Notes = "Notes ...",
                            UserId = 1
                        };
                        context.Products.Add(newProduct);
                        context.SaveChanges(); // it uses a transaction behind the scene.
                    }
                    catch (Exception ex)
                    {
                        // NOTE: This doesn't work with `EntityFrameworkInMemoryDatabase`. Because it doesn't support constraints.
                        // ProductName is duplicate here and should throw an exception on save changes
                        // and rollback the transaction automatically.
                        Console.WriteLine(ex.ToString());
                    }

                    var list3 = context.Products.Include(x => x.TagProducts).ThenInclude(x => x.Tag)
                        .OrderBy(product => product.ProductNumber)
                        .Where(product => product.IsActive == isActive && product.ProductName == name)
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .ToList();
                    Assert.AreEqual(1, loggerProvider.GetCacheHitCount());
                    Assert.IsTrue(list3.Any());
                });
        }

        [DataTestMethod]
        [DataRow(TestCacheProvider.BuiltInInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
        public void TestRemoveDataShouldInvalidateTheCacheAutomatically(TestCacheProvider cacheProvider)
        {
            EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, false, (context, loggerProvider) =>
               {
                   var isActive = false;
                   var name = "Product4";

                   var list1 = context.Products.Include(x => x.TagProducts).ThenInclude(x => x.Tag)
                       .OrderBy(product => product.ProductNumber)
                       .Where(product => product.IsActive == isActive && product.ProductName == name)
                       .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                       .ToList();
                   Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                   Assert.IsNotNull(list1);

                   var list2 = context.Products.Include(x => x.TagProducts).ThenInclude(x => x.Tag)
                       .OrderBy(product => product.ProductNumber)
                       .Where(product => product.IsActive == isActive && product.ProductName == name)
                       .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                       .ToList();
                   Assert.AreEqual(1, loggerProvider.GetCacheHitCount());
                   Assert.IsTrue(list2.Any());

                   var product1 = context.Products.First(product => product.ProductName == name);
                   product1.Notes = $"Test ... {RandomNumberProvider.Next()}";
                   context.SaveChanges();

                   var list3 = context.Products.Include(x => x.TagProducts).ThenInclude(x => x.Tag)
                       .OrderBy(product => product.ProductNumber)
                       .Where(product => product.IsActive == isActive && product.ProductName == name)
                       .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                       .ToList();
                   Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                   Assert.IsNotNull(list3);
               });
        }

        [DataTestMethod]
        [DataRow(TestCacheProvider.BuiltInInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
        public void TestRemoveTptDataShouldInvalidateTheCacheAutomatically(TestCacheProvider cacheProvider)
        {
            EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, false,
                (context, loggerProvider) =>
                {
                    var list1 = context.Posts.OfType<Page>().Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45)).ToList();
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());

                    var list2 = context.Posts.OfType<Page>().Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45)).ToList();
                    Assert.AreEqual(1, loggerProvider.GetCacheHitCount());

                    var post1 = context.Posts.First(post => post.Id == 1);
                    post1.Title = $"Post{RandomNumberProvider.Next()}";
                    context.SaveChanges();

                    var list3 = context.Posts.OfType<Page>().Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45)).ToList();
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                });
        }

        [DataTestMethod]
        [DataRow(TestCacheProvider.BuiltInInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
        public void TestAddThenRemoveDataShouldInvalidateTheCacheAutomatically(TestCacheProvider cacheProvider)
        {
            EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, false,
                (context, loggerProvider) =>
                {
                    User user1;
                    const string user1Name = "User1";
                    if (!context.Users.Any(user => user.Name == user1Name))
                    {
                        user1 = new User
                        {
                            Name = user1Name,
                            AddDate = new DateTime(2020, 4, 3, 17, 50, 39, 503),
                            UpdateDate = null,
                            Points = 1000,
                            IsActive = true,
                            ByteValue = 1,
                            CharValue = 'C',
                            DateTimeOffsetValue = new DateTimeOffset(new DateTime(2020, 4, 3, 17, 50, 39, 503)),
                            DecimalValue = 1.1M,
                            DoubleValue = 1.3,
                            FloatValue = 1.2f,
                            GuidValue = new Guid("236bbe40-b861-433c-8789-b152a99cfe3e"),
                            TimeSpanValue = new TimeSpan(1, 0, 0, 0, 0),
                            ShortValue = 2,
                            ByteArrayValue = new byte[] { 1, 2 },
                            UintValue = 1,
                            UlongValue = 1,
                            UshortValue = 1
                        };
                        user1 = context.Users.Add(user1).Entity;
                    }
                    else
                    {
                        user1 = context.Users.First(user => user.Name == user1Name);
                    }

                    var userOne = context.Users.Cacheable().First(user => user.Name == user1Name);
                    userOne = context.Users.Cacheable().First(user => user.Name == user1Name);
                    Assert.IsNotNull(userOne);
                    loggerProvider.ClearItems();

                    var product = new Product
                    {
                        IsActive = false,
                        ProductName = $"Product{RandomNumberProvider.Next()}",
                        ProductNumber = RandomNumberProvider.Next().ToString(),
                        Notes = "Notes ...",
                        UserId = 1
                    };
                    context.Products.Add(product);
                    context.SaveChanges();

                    var p98 = context.Products
                                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                                    .FirstOrDefault(p => p.ProductId == product.ProductId);
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                    Assert.IsNotNull(p98);

                    var firstQueryWithWhereClauseResult = context.Products.Where(p => p.ProductId == product.ProductId)
                                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                                    .FirstOrDefault();
                    Assert.AreEqual(1, loggerProvider.GetCacheHitCount());
                    Assert.IsNotNull(firstQueryWithWhereClauseResult);

                    context.Products.Remove(product);
                    context.SaveChanges();

                    p98 = context.Products
                                .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                                .FirstOrDefault(p => p.ProductId == product.ProductId);
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                    Assert.IsNull(p98);

                    var firstQueryWithWhereClauseResult2 = context.Products.Where(p => p.ProductId == product.ProductId)
                                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                                    .FirstOrDefault();
                    Assert.AreEqual(1, loggerProvider.GetCacheHitCount());
                    Assert.IsNull(firstQueryWithWhereClauseResult2);

                    p98 = context.Products.FirstOrDefault(p => p.ProductId == product.ProductId);
                    Assert.IsNull(p98);
                });
        }


        [DataTestMethod]
        [DataRow(TestCacheProvider.BuiltInInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreInMemory)]
        [DataRow(TestCacheProvider.CacheManagerCoreRedis)]
        public void TestCachingByteArrays(TestCacheProvider cacheProvider)
        {
            EFServiceProvider.RunInContext(cacheProvider, LogLevel.Debug, false,
                (context, loggerProvider) =>
                {
                    User user1;
                    const string user1Name = "User1";
                    var binaryData = new byte[] { 1, 2, 3 };

                    if (!context.Users.Any(user => user.Name == user1Name))
                    {
                        user1 = new User { Name = user1Name, ImageData = binaryData };
                        user1 = context.Users.Add(user1).Entity;
                    }
                    else
                    {
                        user1 = context.Users.First(user => user.Name == user1Name);
                        user1.ImageData = binaryData;
                    }
                    context.SaveChanges();

                    var userData = context.Users
                                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                                    .FirstOrDefault(p => p.Id == user1.Id);
                    Assert.AreEqual(0, loggerProvider.GetCacheHitCount());
                    Assert.IsNotNull(userData);
                    CollectionAssert.AreEqual(binaryData, userData.ImageData);

                    userData = context.Users
                                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                                    .FirstOrDefault(p => p.Id == user1.Id);
                    Assert.AreEqual(1, loggerProvider.GetCacheHitCount());
                    Assert.IsNotNull(userData);
                    CollectionAssert.AreEqual(binaryData, userData.ImageData);
                });
        }
    }
}