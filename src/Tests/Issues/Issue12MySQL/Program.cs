using System.Linq;
using EFCoreSecondLevelCacheInterceptor;
using Microsoft.EntityFrameworkCore;
using System;
using Issue12MySQL.Entities;

namespace Issue12MySQL
{
    /*
    From https://github.com/dotnet/efcore/issues/17788#issuecomment-574569529

    There are two MySQL providers for Entity Framework Core:
    - The official one from MySQL: MySql.Data.EntityFrameworkCore. As of now, the latest version is 8.0.19, and works with Entity Framework Core 2.1 (and probably also 2.2). Since EF Core 3.0 is a major version with breaking changes, you cannot use it with this provider.
    - The Pomelo provider: Pomelo.EntityFrameworkCore.MySql. There is a 3.1 version of this provider.
    In other words, if you want to use EF Core 3.0/3.1 with MySQL, at this point you need to use the Pomelo provider (or wait for the official MySQL one to get released).
    */
    class Program
    {
        static void Main(string[] args)
        {
            initDb();

            EFServiceProvider.RunInContext(context =>
            {
                var people = context.People.ToList();
                foreach (var person in people)
                {
                    Console.WriteLine($"{person.Id}, {person.Name}");
                }

                var cachedPeople = context.People.Cacheable().ToList();
                cachedPeople = context.People.Cacheable().ToList();
                foreach (var person in cachedPeople)
                {
                    Console.WriteLine($"{person.Id}, {person.Name}");
                }

                cachedPeople = context.People.Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(51)).ToList();
                cachedPeople = context.People.Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(51)).ToList();
                foreach (var person in cachedPeople)
                {
                    Console.WriteLine($"{person.Id}, {person.Name}");
                }
            });
        }

        private static void initDb()
        {
            EFServiceProvider.RunInContext(context =>
            {
                context.Database.Migrate();

                if (!context.People.Any())
                {
                    context.People.Add(new Person
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

                    context.People.Add(new Person
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

                    context.SaveChanges();
                }
            });
        }
    }
}