using Issue123WithMessagePack.Entities;
using Microsoft.EntityFrameworkCore;

namespace Issue123WithMessagePack.DataLayer
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Person> People { get; set; }
    }
}