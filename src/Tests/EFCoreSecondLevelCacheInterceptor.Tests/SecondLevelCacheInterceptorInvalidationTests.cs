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
        [DataRow(false)]
        [DataRow(true)]
        public void TestInsertingDataIntoTheSameTableShouldInvalidateTheCacheAutomatically(bool useRedis)
        {
            EFServiceProvider.RunInContext(useRedis, LogLevel.Information, false,
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
        [DataRow(false)]
        [DataRow(true)]
        public void TestInsertingDataToOtherTablesShouldNotInvalidateTheCacheDependencyAutomatically(bool useRedis)
        {
            EFServiceProvider.RunInContext(useRedis, LogLevel.Information, false, (context, loggerProvider) =>
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
        [DataRow(false)]
        [DataRow(true)]
        public void TestInsertingDataToRelatedTablesShouldInvalidateTheCacheDependencyAutomatically(bool useRedis)
        {
            EFServiceProvider.RunInContext(useRedis, LogLevel.Information, false,
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
        [DataRow(false)]
        [DataRow(true)]
        public void TestTransactionRollbackShouldNotInvalidateTheCacheDependencyAutomatically(bool useRedis)
        {
            EFServiceProvider.RunInContext(useRedis, LogLevel.Information, false,
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
        [DataRow(false)]
        [DataRow(true)]
        public void TestRemoveDataShouldInvalidateTheCacheAutomatically(bool useRedis)
        {
            EFServiceProvider.RunInContext(useRedis, LogLevel.Information, false, (context, loggerProvider) =>
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
        [DataRow(false)]
        [DataRow(true)]
        public void TestRemoveTptDataShouldInvalidateTheCacheAutomatically(bool useRedis)
        {
            EFServiceProvider.RunInContext(useRedis, LogLevel.Information, false,
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
        [DataRow(false)]
        [DataRow(true)]
        public void TestAddThenRemoveDataShouldInvalidateTheCacheAutomatically(bool useRedis)
        {
            EFServiceProvider.RunInContext(useRedis, LogLevel.Information, false,
                (context, loggerProvider) =>
                {
                    User user1;
                    const string user1Name = "User1";
                    if (!context.Users.Any(user => user.Name == user1Name))
                    {
                        user1 = new User { Name = user1Name };
                        user1 = context.Users.Add(user1).Entity;
                    }
                    else
                    {
                        user1 = context.Users.First(user => user.Name == user1Name);
                    }

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
        [DataRow(false)]
        [DataRow(true)]
        public void TestCachingByteArrays(bool useRedis)
        {
            EFServiceProvider.RunInContext(useRedis, LogLevel.Information, false,
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