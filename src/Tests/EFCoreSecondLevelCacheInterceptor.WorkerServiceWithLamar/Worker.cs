using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer;
using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EFCoreSecondLevelCacheInterceptor.WorkerServiceWithLamar
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;

            // Note: Don't inject the `ApplicationDbContext` here. Because it will become a `singleton`.
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            await RunInContextAsync(async context =>
            {
                var post1 = await context.Set<Post>()
                    .Include(post => post.User)
                    .Where(post => post.Id > 0)
                    .OrderBy(post => post.Id)
                    .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                    .FirstOrDefaultAsync();

                post1 = await context.Set<Post>()
                            .Include(post => post.User)
                            .Where(post => post.Id > 0)
                            .OrderBy(post => post.Id)
                            .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                            .FirstOrDefaultAsync();
                _logger.LogInformation($"Title: {post1.Title}");
            });
        }

        private void RunInContext(Action<ApplicationDbContext> action)
        {
            using var serviceScope = _serviceScopeFactory.CreateScope();
            using var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            action(context);
        }

        private async Task RunInContextAsync(Func<ApplicationDbContext, Task> action)
        {
            using var serviceScope = _serviceScopeFactory.CreateScope();
            using var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await action(context);
        }
    }
}