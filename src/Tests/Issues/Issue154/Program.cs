using EFCoreSecondLevelCacheInterceptor;
using Issue154.Entities;
using Microsoft.EntityFrameworkCore;

namespace Issue154;

internal static class Program
{
    private static void Main(string[] args)
    {
        initDb();

        EFServiceProvider.RunInContext(context =>
        {
            Console.WriteLine(
                value:
                "Not using the `Cacheable()` method with options.UseMemoryCacheProvider(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(5)).");

            var people = context.People.ToList();
            people = context.People.ToList();
            people = context.People.ToList();

            foreach (var person in people)
            {
                Console.WriteLine($"{person.Id}, {person.Name}");
            }

            Console.WriteLine(
                value:
                "Not using the `Cacheable()` method with options.UseMemoryCacheProvider(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(5)).");

            people = context.People.ToList();
            people = context.People.ToList();

            foreach (var person in people)
            {
                Console.WriteLine($"{person.Id}, {person.Name}");
            }

            Console.WriteLine(value: "Using the `Cacheable()` method.");
            var cachedPeople = context.People.Cacheable().ToList();

            cachedPeople = context.People.Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 1))
                .ToList();

            cachedPeople = context.People.Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 1))
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
                    Name = "Bill"
                });

                context.People.Add(new Person
                {
                    Name = "Vahid"
                });

                context.SaveChanges();
            }
        });
}