using Issue9SQLiteInt32.Entities;
using Microsoft.EntityFrameworkCore;

namespace Issue9SQLiteInt32.DataLayer;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Person> People { get; set; }
}