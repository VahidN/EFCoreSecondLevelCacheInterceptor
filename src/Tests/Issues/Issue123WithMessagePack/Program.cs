using System;
using System.Linq;
using EFCoreSecondLevelCacheInterceptor;
using Issue123WithMessagePack.Entities;
using Microsoft.EntityFrameworkCore;

namespace Issue123WithMessagePack;

internal class Program
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
                                               Console
                                                   .WriteLine($"Person: {person.Id}, {person.Name}, {person.Date}, {person.DateOffset}, {person.Span}, {person.DateOnly}, {person.TimeOnly}");
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
                                               var now = DateTime.Now;
                                               context.People.Add(new Person
                                                                  {
                                                                      Name = "Bill",
                                                                      Date = now,
                                                                      DateOffset = DateTimeOffset.Now,
                                                                      Span = TimeSpan.FromMinutes(2),
                                                                      DateOnly = DateOnly.FromDateTime(now),
                                                                      TimeOnly =
                                                                          TimeOnly
                                                                              .FromTimeSpan(TimeSpan.FromMinutes(6)),
                                                                  });
                                               context.People.Add(new Person
                                                                  {
                                                                      Name = null,
                                                                      Date = now,
                                                                      DateOffset = DateTimeOffset.Now,
                                                                      Span = TimeSpan.FromMinutes(1),
                                                                      DateOnly = DateOnly.FromDateTime(now),
                                                                      TimeOnly =
                                                                          TimeOnly
                                                                              .FromTimeSpan(TimeSpan.FromMinutes(6)),
                                                                  });
                                               context.SaveChanges();
                                           }
                                       });
    }
}