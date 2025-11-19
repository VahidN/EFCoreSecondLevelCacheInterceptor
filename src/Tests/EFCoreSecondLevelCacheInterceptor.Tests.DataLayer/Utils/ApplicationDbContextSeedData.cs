using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Utils;

public static class ApplicationDbContextSeedData
{
    public static void SeedData(this IServiceScopeFactory scopeFactory)
    {
        using var serviceScope = scopeFactory.CreateScope();
        using var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        addSP(context);
        addUser(context);
        addDateType(context);
    }

    private static void addDateType(ApplicationDbContext context)
    {
        var rnd = new Random();
        var isEven = rnd.Next(minValue: 1, maxValue: 100000) % 2 == 0;
        var now = DateTime.UtcNow;

        var dateType1 = new DateType
        {
            AddDate = isEven ? null : now,
            UpdateDate = now,
            AddDateValue = isEven ? null : DateTimeOffset.UtcNow,
            UpdateDateValue = DateTimeOffset.UtcNow,
            RelativeAddTimeValue = isEven ? null : TimeSpan.FromMinutes(minutes: 6),
            RelativeUpdateTimeValue = TimeSpan.FromMinutes(minutes: 6),
            AddDateOnlyValue = isEven ? null : DateOnly.FromDateTime(now),
            UpdateDateOnlyValue = DateOnly.FromDateTime(now),
            RelativeAddTimeOnlyValue = isEven ? null : TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(minutes: 6)),
            RelativeUpdateTimeOnlyValue = TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(minutes: 6))
        };

        context.DateTypes.Add(dateType1);
        context.SaveChanges();
    }

    private static void addUser(ApplicationDbContext context)
    {
        var rnd = new Random();

        var user1 = new User
        {
            Name = $"User {rnd.Next(minValue: 1, maxValue: 100000)}",
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
            TimeSpanValue = TimeSpan.FromMinutes(minutes: 1),
            ShortValue = 2,
            ByteArrayValue = new byte[]
            {
                1, 2
            },
            UintValue = 1,
            UlongValue = 1,
            UshortValue = 1,
            ImageData = []
        };

        context.Users.Add(user1);
        context.SaveChanges();
    }

    private static void addSP(ApplicationDbContext context)
    {
        // Note: You should add it to your migrations `void Up` method manually later.

        context.Database.ExecuteSqlRaw(sql: @"IF EXISTS ( SELECT *
            FROM   sysobjects
            WHERE  id = object_id(N'[dbo].[usp_GetBlogData]')
                   and OBJECTPROPERTY(id, N'IsProcedure') = 1 )
BEGIN
    DROP PROCEDURE [dbo].[usp_GetBlogData]
END");

        context.Database.ExecuteSqlRaw(sql: @"CREATE PROCEDURE [dbo].[usp_GetBlogData]
 @BlogId int

AS
BEGIN
  SET NOCOUNT ON;

  SELECT BlogId as Id, Url as SiteUrl from Blogs
  where BlogId = @BlogId
END");
    }
}