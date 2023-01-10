using Issue192.Entities;
using Microsoft.EntityFrameworkCore;

namespace Issue192.DataLayer;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<Person> People { get; set; }
}