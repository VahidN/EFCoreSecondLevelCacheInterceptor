using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer;
using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace EFCoreSecondLevelCacheInterceptor.WorkerServiceWithLamar;

public class Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    // Note: Don't inject the `ApplicationDbContext` here. Because it will become a `singleton`.

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(message: "Worker running at: {Time}", DateTimeOffset.Now);

        await RunInContextAsync(async context =>
        {
            var post1 = await context.Set<Post>()
                .Include(post => post.User)
                .Where(post => post.Id > 0)
                .OrderBy(post => post.Id)
                .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                .FirstOrDefaultAsync(stoppingToken);

            post1 = await context.Set<Post>()
                .Include(post => post.User)
                .Where(post => post.Id > 0)
                .OrderBy(post => post.Id)
                .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 45))
                .FirstOrDefaultAsync(stoppingToken);

            logger.LogInformation(message: "Title: {Post1Title}", post1?.Title);
        });
    }

    private async Task RunInContextAsync(Func<ApplicationDbContext, Task> action)
    {
        using var serviceScope = serviceScopeFactory.CreateScope();
        await using var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await action(context);
    }
}