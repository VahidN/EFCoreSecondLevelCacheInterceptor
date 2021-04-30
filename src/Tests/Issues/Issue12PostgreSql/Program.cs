using System.Linq;
using EFCoreSecondLevelCacheInterceptor;
using Microsoft.EntityFrameworkCore;
using System;
using Issue12PostgreSql.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;
using Issue12PostgreSql.DataLayer;

namespace Issue12PostgreSql
{
    class Program
    {
        static void Main(string[] args)
        {
            initDb();

            EFServiceProvider.RunInContext(context =>
            {
                testArrays(context);

                var people = context.People.Include(x => x.Addresses).Include(x => x.Books).ToList();
                foreach (var person in people)
                {
                    Console.WriteLine($"{person.Id}, {person.Name}, {person.Addresses.First().Name}");
                }

                var cachedPeople = context.People.Include(x => x.Addresses).Include(x => x.Books).Cacheable().ToList();
                cachedPeople = context.People.Include(x => x.Addresses).Include(x => x.Books).Cacheable().ToList();
                foreach (var person in cachedPeople)
                {
                    Console.WriteLine($"{person.Id}, {person.Name}");
                }

                cachedPeople = context.People.Include(x => x.Addresses).Include(x => x.Books).Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(51)).ToList();
                cachedPeople = context.People.Include(x => x.Addresses).Include(x => x.Books).Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(51)).ToList();
                foreach (var person in cachedPeople)
                {
                    Console.WriteLine($"{person.Id}, {person.Name}, {person.Addresses.First().Name}");
                }
            });
        }

        private static void testArrays(ApplicationDbContext context)
        {
            var firstQueryResult = queryEntities(context, new int[] { 1, 3 });
            foreach (var entity in firstQueryResult)
            {
                Console.WriteLine($"firstQueryResult -> Id: {entity.Id}");
            }

            var secondQueryResult = queryEntities(context, new int[] { 4, 8, 9 });
            foreach (var entity in secondQueryResult)
            {
                Console.WriteLine($"secondQueryResult -> Id: {entity.Id}");
            }
        }

        private static List<Entity> queryEntities(ApplicationDbContext dbContext, int[] array)
        {
            return dbContext.Entities.AsNoTracking()
                            .Where(entity => entity.Array.Any(x => array.Contains(x))).Cacheable().ToList();
        }

        private static void initDb()
        {
            EFServiceProvider.RunInContext(context =>
            {
                context.Database.Migrate();

                if (!context.People.Any())
                {
                    var person1 = context.People.Add(new Person
                    {
                        Name = "Bill",
                        AddDate = DateTime.UtcNow,
                        UpdateDate = null,
                        Points = 1000,
                        IsActive = true,
                        ByteValue = 1,
                        CharValue = 'C',
                        DateTimeOffsetValue = DateTimeOffset.UtcNow,
                        DecimalValue = 1.1M,
                        DoubleValue = 1.3,
                        FloatValue = 1.2f,
                        GuidValue = Guid.NewGuid(),
                        TimeSpanValue = TimeSpan.FromDays(1),
                        ShortValue = 2,
                        ByteArrayValue = new byte[] { 1, 2 },
                        UintValue = 1,
                        UlongValue = 1,
                        UshortValue = 1
                    });

                    context.Addresses.Add(new Address { Name = "Addr 1", Person = person1.Entity });
                    context.Books.Add(new Book { Name = "Book 1", Person = person1.Entity });

                    var person2 = context.People.Add(new Person
                    {
                        Name = "Vahid",
                        AddDate = DateTime.UtcNow,
                        UpdateDate = null,
                        Points = 1000,
                        IsActive = true,
                        ByteValue = 1,
                        CharValue = 'C',
                        DateTimeOffsetValue = DateTimeOffset.UtcNow,
                        DecimalValue = 1,
                        DoubleValue = 2,
                        FloatValue = 3,
                        GuidValue = Guid.NewGuid(),
                        TimeSpanValue = TimeSpan.FromDays(1),
                        ShortValue = 2,
                        ByteArrayValue = new byte[] { 1, 2 },
                        UintValue = 1,
                        UlongValue = 1,
                        UshortValue = 1
                    });

                    context.Addresses.Add(new Address { Name = "Addr 2", Person = person2.Entity });
                    context.Books.Add(new Book { Name = "Book 2", Person = person2.Entity });

                    context.SaveChanges();
                }

                if (!context.Entities.Any())
                {
                    var initialData = new[]
                    {
                        new Entity { Array = new[] { 1, 2, 3 } },
                        new Entity { Array = new[] { 4, 5, 6 } },
                        new Entity { Array = new[] { 7, 8, 9 } }
                    };
                    context.AddRange(initialData);

                    context.SaveChanges();
                }
            });
        }
    }
}