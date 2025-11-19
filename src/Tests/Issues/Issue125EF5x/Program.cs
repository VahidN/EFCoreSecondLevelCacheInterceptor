using EFCoreSecondLevelCacheInterceptor;
using Issue125EF5x.Entities;
using Microsoft.EntityFrameworkCore;

namespace Issue125EF5x;

internal static class Program
{
    private static void Main(string[] args)
    {
        initDb();

        EFServiceProvider.GetRequiredService<IEFCacheServiceProvider>().ClearAllCachedEntries();

        EFServiceProvider.RunInContext(context =>
        {
            var cachedPeople = context.People.ToList();
            cachedPeople = context.People.ToList();

            foreach (var person in cachedPeople)
            {
                Console.WriteLine($"{person.Id}, {person.Name}");
            }

            var person1 = context.People.Single(x => x.Id == 1);
            person1 = context.People.Single(x => x.Id == 1);
            Console.WriteLine($"{person1.Id}, {person1.Name}");
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

                context.SaveChanges();
            }
        });
}