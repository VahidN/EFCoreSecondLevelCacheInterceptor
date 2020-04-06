using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer;
using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace EFCoreSecondLevelCacheInterceptor.AspNetCoreSampleWithLamar.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var param1 = 0;
            var post1 = await _context.Set<Post>()
                .Include(post => post.User)
                .Where(post => post.Id > param1)
                .OrderBy(post => post.Id)
                .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                .FirstOrDefaultAsync();
            return Json(new { post1.Title, post1.User.Name });
        }
    }
}
