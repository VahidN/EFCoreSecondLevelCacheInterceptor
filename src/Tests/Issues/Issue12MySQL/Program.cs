using EFCoreSecondLevelCacheInterceptor;
using Issue12MySQL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Issue12MySQL;

internal static class Program
{
    private static void Main(string[] args)
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

            cachedPeople = context.People.Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 51))
                .ToList();

            cachedPeople = context.People.Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 51))
                .ToList();

            foreach (var person in cachedPeople)
            {
                Console.WriteLine($"{person.Id}, {person.Name}");
            }
        });
    }

    private static void initDb()
        => EFServiceProvider.RunInContext(context =>
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
                    TimeSpanValue = TimeSpan.FromDays(days: 1),
                    ShortValue = 2,
                    ByteArrayValue = new byte[]
                    {
                        1, 2
                    },
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
                    TimeSpanValue = TimeSpan.FromDays(days: 1),
                    ShortValue = 2,
                    ByteArrayValue = new byte[]
                    {
                        1, 2
                    },
                    UintValue = 1,
                    UlongValue = 1,
                    UshortValue = 1
                });

                context.SaveChanges();
            }
        });
}