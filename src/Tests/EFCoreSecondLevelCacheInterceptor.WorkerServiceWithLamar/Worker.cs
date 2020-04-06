using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer;
using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EFCoreSecondLevelCacheInterceptor.WorkerServiceWithLamar
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ApplicationDbContext _context;

        public Worker(ILogger<Worker> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            var post1 = await _context.Set<Post>()
                .Include(post => post.User)
                .Where(post => post.Id > 0)
                .OrderBy(post => post.Id)
                .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                .FirstOrDefaultAsync();

            post1 = await _context.Set<Post>()
                        .Include(post => post.User)
                        .Where(post => post.Id > 0)
                        .OrderBy(post => post.Id)
                        .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                        .FirstOrDefaultAsync();
            _logger.LogInformation($"Title: {post1.Title}");
        }
    }
}