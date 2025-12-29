using EFCoreSecondLevelCacheInterceptor;
using Issue12PostgreSql;
using Issue12PostgreSql.DataLayer;
using Issue12PostgreSql.Entities;
using Microsoft.EntityFrameworkCore;

InitDb();

EFServiceProvider.RunInContext(context =>
{
    TestLists(context);
    TestArrays(context);
    TestPeople(context);
});

static void TestPeople(ApplicationDbContext context)
{
    var people = context.People.Include(x => x.Addresses).Include(x => x.Books).ToList();

    foreach (var person in people)
    {
        Console.WriteLine($"{person.Id}, {person.Name}, {person.Addresses?.First().Name}");
    }

    var cachedPeople = context.People.Include(x => x.Addresses).Include(x => x.Books).Cacheable().ToList();
    cachedPeople = context.People.Include(x => x.Addresses).Include(x => x.Books).Cacheable().ToList();

    foreach (var person in cachedPeople)
    {
        Console.WriteLine($"{person.Id}, {person.Name}, {person.CustomFieldDefinitionMetadata?.FieldName}");
    }

    cachedPeople = context.People.Include(x => x.Addresses)
        .Include(x => x.Books)
        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 51))
        .ToList();

    cachedPeople = context.People.Include(x => x.Addresses)
        .Include(x => x.Books)
        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 51))
        .ToList();

    foreach (var person in cachedPeople)
    {
        Console.WriteLine($"{person.Id}, {person.Name}, {person.Addresses?.First().Name}");
    }
}

static void TestArrays(ApplicationDbContext context)
{
    var firstQueryResult = QueryArrays(context, [1, 3]);

    foreach (var entity in firstQueryResult)
    {
        Console.WriteLine($"firstArrayQueryResult -> Id: {entity.Id}");
    }

    var secondQueryResult = QueryArrays(context, [4, 8, 9]);

    foreach (var entity in secondQueryResult)
    {
        Console.WriteLine($"secondArrayQueryResult -> Id: {entity.Id}");
    }
}

static List<Entity> QueryArrays(ApplicationDbContext dbContext, int[] array)
    => dbContext.Entities.AsNoTracking()
        .Where(entity => entity.Array != null && entity.Array.Any(x => array.Contains(x)))
        .Cacheable()
        .ToList();

static void TestLists(ApplicationDbContext context)
{
    var firstQueryResult = QueryLists(context, ["1", "2"]);

    foreach (var entity in firstQueryResult)
    {
        Console.WriteLine($"firstListQueryResult -> Id: {entity.Id}");
    }

    var secondQueryResult = QueryLists(context, ["3", "4"]);

    foreach (var entity in secondQueryResult)
    {
        Console.WriteLine($"secondListQueryResult -> Id: {entity.Id}");
    }
}

static List<Entity> QueryLists(ApplicationDbContext dbContext, List<string> list)
    => dbContext.Entities.AsNoTracking()
        .Where(entity => entity.List != null && entity.List.Any(x => list.Contains(x)))
        .Cacheable()
        .ToList();

static void InitDb()
    => EFServiceProvider.RunInContext(context =>
    {
        context.Database.Migrate();
        AddPeople(context);
        AddEntities(context);
    });

static void AddPeople(ApplicationDbContext context)
{
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
            TimeSpanValue = TimeSpan.FromDays(days: 1),
            ShortValue = 2,
            ByteArrayValue = [1, 2],
            UintValue = 1,
            UlongValue = 1,
            UshortValue = 1,
            Date1 = new DateOnly(year: 2021, month: 09, day: 23),
            Date2 = null,
            Time1 = TimeOnly.FromTimeSpan(new DateTime(year: 2021, month: 09, day: 23, hour: 19, minute: 2, second: 0) -
                                          new DateTime(year: 2021, month: 09, day: 23, hour: 6, minute: 54, second: 0)),
            Time2 = null
        });

        context.Addresses.Add(new Address
        {
            Name = "Addr 1",
            Person = person1.Entity
        });

        context.Books.Add(new Book
        {
            Name = "Book 1",
            Person = person1.Entity
        });

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
            TimeSpanValue = TimeSpan.FromDays(days: 1),
            ShortValue = 2,
            ByteArrayValue = [1, 2],
            UintValue = 1,
            UlongValue = 1,
            UshortValue = 1,
            OptionDefinitions =
            [
                new BlogOption
                {
                    IsActive = true,
                    Name = "Test",
                    NumberOfTimesUsed = 1,
                    SortOrder = 1
                }
            ],
            CustomFieldDefinitionMetadata = new CustomFieldDefinitionMetadata
            {
                FieldName = "Id",
                FieldType = "int"
            }
        });

        context.Addresses.Add(new Address
        {
            Name = "Addr 2",
            Person = person2.Entity
        });

        context.Books.Add(new Book
        {
            Name = "Book 2",
            Person = person2.Entity
        });

        context.SaveChanges();
    }
}

static void AddEntities(ApplicationDbContext context)
{
    if (!context.Entities.Any())
    {
        var initialData = new[]
        {
            new Entity
            {
                Array = [1, 2, 3],
                List = ["1", "2"]
            },
            new Entity
            {
                Array = [4, 5, 6],
                List = ["3", "4"]
            },
            new Entity
            {
                Array = null,
                List = null
            }
        };

        context.AddRange(initialData);

        context.SaveChanges();
    }
}