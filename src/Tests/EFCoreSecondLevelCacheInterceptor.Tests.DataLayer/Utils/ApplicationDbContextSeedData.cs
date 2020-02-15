using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Utils
{
    public static class ApplicationDbContextSeedData
    {
        public static void SeedData(this IServiceScopeFactory scopeFactory)
        {
            using (var serviceScope = scopeFactory.CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>())
                {
                    addSP(context);
                }
            }
        }

        private static void addSP(ApplicationDbContext context)
        {
            // Note: You should add it to your migrations `void Up` method manually later.

            context.Database.ExecuteSqlRaw(
@"IF EXISTS ( SELECT *
            FROM   sysobjects
            WHERE  id = object_id(N'[dbo].[usp_GetBlogData]')
                   and OBJECTPROPERTY(id, N'IsProcedure') = 1 )
BEGIN
    DROP PROCEDURE [dbo].[usp_GetBlogData]
END");

            context.Database.ExecuteSqlRaw(
@"CREATE PROCEDURE [dbo].[usp_GetBlogData]
 @BlogId int

AS
BEGIN
  SET NOCOUNT ON;

  SELECT BlogId as Id, Url as SiteUrl from Blogs
  where BlogId = @BlogId
END");
        }
    }
}