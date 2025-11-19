using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Entities;
using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace EFCoreSecondLevelCacheInterceptor.ConsoleSample;

internal static class Program
{
    private static void Main(string[] args)
    {
        initDb();

        EFServiceProvider.RunInContext(context =>
        {
            context.Posts.Add(new Post
            {
                Title = "Title 1",
                UserId = 1
            });

            context.SaveChanges();

            var posts = context.Posts.Cacheable().ToList();
            Console.WriteLine($"Title From DB: {posts[index: 0].Title}");

            posts = context.Posts.Cacheable().ToList();
            Console.WriteLine($"Title From Cache: {posts[index: 0].Title}");
        });
    }

    private static void initDb()
    {
        var serviceScope = EFServiceProvider.GetRequiredService<IServiceScopeFactory>();
        serviceScope.Initialize();
        serviceScope.SeedData();
    }
}