using Issue12PostgreSql.Entities;
using Microsoft.EntityFrameworkCore;

namespace Issue12PostgreSql.DataLayer;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Person> People { get; set; }

    public DbSet<Address> Addresses { get; set; }

    public DbSet<Book> Books { get; set; }

    public DbSet<Entity> Entities { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableSensitiveDataLogging().EnableDetailedErrors();

        base.OnConfiguring(optionsBuilder);
    }
}