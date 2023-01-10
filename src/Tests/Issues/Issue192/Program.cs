using System.Linq;
using EFCore.BulkExtensions;
using EFCoreSecondLevelCacheInterceptor;
using Issue192.Entities;
using Microsoft.EntityFrameworkCore;

namespace Issue192;

internal class Program
{
    private static void Main(string[] args)
    {
        initDb();

        EFServiceProvider.GetRequiredService<IEFCacheServiceProvider>().ClearAllCachedEntries();

        EFServiceProvider.RunInContext(context =>
                                       {
                                           context.People.Where(a => a.Id > 500).BatchDelete();

                                           context.People.Where(a => a.Id <= 500)
                                                  .BatchUpdate(a => new Person { Name = a.Name + "-Test" });

                                           //`EFCore.BulkExtensions` doesn't pass all of its executed SQL commands such as `BulkInsert` through the interceptors.
                                           //So ... it can't be detected here.                                            

                                           context.BulkInsertOrUpdate(new[]
                                                                      {
                                                                          new Person
                                                                          {
                                                                              Name = "Bill 2",
                                                                          },
                                                                          new Person
                                                                          {
                                                                              Name = "Bill 3",
                                                                          },
                                                                      });
                                           /*
MERGE [dbo].[People] WITH (HOLDLOCK) AS T USING (SELECT TOP 2 * FROM [dbo].[PeopleTemp94f5cba8] ORDER BY [Id]) AS S ON T.[Id] = S.[Id] WHEN NOT MATCHED BY TARGET THEN INSERT ([Name]) VALUES (S.[Name]) WHEN MATCHED AND EXISTS (SELECT S.[Name] EXCEPT SELECT T.[Name]) THEN UPDATE SET T.[Name] = S.[Name];                                             
                                            */
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
                                                                      Name = "Bill 1",
                                                                  });
                                               context.SaveChanges();
                                           }
                                       });
    }
}