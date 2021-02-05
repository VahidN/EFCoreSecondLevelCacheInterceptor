using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer;
using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace EFCoreSecondLevelCacheInterceptor.AspNetCoreSample.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> ExplicitIndex()
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

        public async Task<IActionResult> Index()
        {
            var param1 = 0;
            var post1 = await _context.Set<Post>()
                .Include(post => post.User)
                .Where(post => post.Id > param1)
                .OrderBy(post => post.Id)
                //.Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45)) --> using `CacheQueriesContainingTypes`
                .FirstOrDefaultAsync();
            return Json(new { post1.Title, post1.User.Name });
        }

        public async Task<IActionResult> CountTest()
        {
            var count = await _context.Posts
                .Where(x => x.Id > 0)
                .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                .CountAsync();
            return Json(new { count });
        }

        public async Task<IActionResult> CountWithParamsTest()
        {
            var count = await _context.Posts
                .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                .CountAsync(x => x.Id > 0);
            return Json(new { count });
        }

        public async Task<IActionResult> CollectionsTest()
        {
            var collection1 = new[] { 1, 2, 3 };
            var post1 = await _context.Posts
                .Where(x => collection1.Contains(x.Id))
                .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                .FirstOrDefaultAsync();

            var collection2 = new[] { 1, 2, 3, 4 };
            var post2 = await _context.Posts
                .Where(x => collection2.Contains(x.Id))
                .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                .FirstOrDefaultAsync();
            return Json(new { post1.Title, post2.Id });
        }


        public IActionResult TestInvalidation()
        {
            User user1;
            const string user1Name = "User1";
            if (!_context.Users.Any(user => user.Name == user1Name))
            {
                user1 = new User { Name = user1Name };
                user1 = _context.Users.Add(user1).Entity;
            }
            else
            {
                user1 = _context.Users.First(user => user.Name == user1Name);
                user1.UserStatus = UserStatus.Disabled;
            }

            var product = new Product
            {
                ProductName = "P981122",
                IsActive = true,
                Notes = "Notes ...",
                ProductNumber = "098112",
                User = user1
            };

            product = _context.Products.Add(product).Entity;
            _context.SaveChanges();

            // 1st query, reading from db
            var firstQueryResult = _context.Products.Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                .FirstOrDefault(p => p.ProductId == product.ProductId);

            var firstQueryWithWhereClauseResult = _context.Products.Where(p => p.ProductId == product.ProductId)
                .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                .FirstOrDefault();

            // Delete it from db, invalidates the cache on SaveChanges
            _context.Products.Remove(product);
            _context.SaveChanges();

            // same query, reading from 2nd level cache? No.
            var secondQueryResult = _context.Products.Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                .FirstOrDefault(p => p.ProductId == product.ProductId);

            // same query, reading from 2nd level cache? No.
            var thirdQueryResult = _context.Products.Where(p => p.ProductId == product.ProductId).Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                .FirstOrDefault();

            // retrieving it directly from database
            var p98 = _context.Products.FirstOrDefault(p => p.ProductId == product.ProductId);

            return Json(new
            {
                firstQueryResult,
                firstQueryWithWhereClauseResult,
                secondQueryResult,
                thirdQueryResult,
                directlyFromDatabase = p98
            });
        }

        public async Task<IActionResult> TestEnumsWithParams()
        {
            var param1 = UserStatus.Active;
            var user1 = await _context.Set<User>().Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                .FirstOrDefaultAsync(x => x.UserStatus == param1);
            return Json(new { user1 });
        }

        public async Task<IActionResult> TestNulls()
        {
            var user1 = await _context.Set<User>().Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45))
                .FirstOrDefaultAsync(x => x.Name == "Test");
            return Json(new { user1 });
        }

        /*public async Task<IActionResult> TestFind()
        {
            var product1 = await _context.Products.Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(45)).FindAsync(1);
            return Json(new { product1.ProductName });
        }*/
    }
}