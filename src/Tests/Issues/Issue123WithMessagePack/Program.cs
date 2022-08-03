using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using Issue123WithMessagePack.Entities;
using EFCoreSecondLevelCacheInterceptor;

namespace Issue123WithMessagePack
{
    class Program
    {
        static void Main(string[] args)
        {
            initDb();

            EFServiceProvider.GetRequiredService<IEFCacheServiceProvider>().ClearAllCachedEntries();

            EFServiceProvider.RunInContext(context =>
            {
                var cachedPeople = context.People.ToList();
                cachedPeople = context.People.ToList();
                foreach (var person in cachedPeople)
                {
                    Console.WriteLine($"{person.Id}, {person.Name}, {person.Date}, {person.DateOffset}");
                }

                var person1 = context.People.Single(x => x.Id == 1);
                person1 = context.People.Single(x => x.Id == 1);
                Console.WriteLine($"{person1.Id}, {person1.Name}");
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
			Date = DateTime.Now,
			DateOffset = DateTimeOffset.Now
                    });
		    context.People.Add(new Person
                    {
                        Name = null,
			Date = DateTime.Now,
			DateOffset = DateTimeOffset.Now						
                    });
                    context.SaveChanges();
                }
            });
        }
    }
}