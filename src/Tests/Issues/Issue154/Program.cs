using System.Linq;
using EFCoreSecondLevelCacheInterceptor;
using Microsoft.EntityFrameworkCore;
using System;
using Issue154.Entities;

namespace Issue154
{
    class Program
    {
        static void Main(string[] args)
        {
            initDb();

            EFServiceProvider.RunInContext(context =>
            {
                Console.WriteLine("Not using the `Cacheable()` method with options.UseMemoryCacheProvider(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(5)).");
                var people = context.People.ToList();
                people = context.People.ToList();
                people = context.People.ToList();
                foreach (var person in people)
                {
                    Console.WriteLine($"{person.Id}, {person.Name}");
                }

                Console.WriteLine("Not using the `Cacheable()` method with options.UseMemoryCacheProvider(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(5)).");
                people = context.People.ToList();
                people = context.People.ToList();
                foreach (var person in people)
                {
                    Console.WriteLine($"{person.Id}, {person.Name}");
                }

                Console.WriteLine("Using the `Cacheable()` method.");
                var cachedPeople = context.People.Cacheable().ToList();
                cachedPeople = context.People.Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(1)).ToList();
                cachedPeople = context.People.Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(1)).ToList();
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
                    });

                    context.People.Add(new Person
                    {
                        Name = "Vahid",
                    });

                    context.SaveChanges();
                }
            });
        }
    }
}