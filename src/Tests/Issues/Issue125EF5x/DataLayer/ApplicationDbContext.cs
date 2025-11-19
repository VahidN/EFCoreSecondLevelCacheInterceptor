using Issue125EF5x.Entities;
using Microsoft.EntityFrameworkCore;

namespace Issue125EF5x.DataLayer;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Person> People { get; set; }
}