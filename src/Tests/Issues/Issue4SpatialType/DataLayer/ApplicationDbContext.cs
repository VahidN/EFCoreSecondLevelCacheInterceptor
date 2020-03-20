using Issue4SpatialType.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Issue4SpatialType.DataLayer
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Person> People { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseLoggerFactory(LoggerFactory.Create(x => x
                    .AddConsole()
                    .AddFilter(y => y >= LogLevel.Debug)))
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Person>(entityBuilder =>
                {
                    entityBuilder.HasKey(x => x.Id);
                    entityBuilder.Property(x => x.Id).ValueGeneratedOnAdd();
                });

            modelBuilder
                .Entity<Product>(entityBuilder =>
                {
                    entityBuilder.HasKey(x => x.Id);
                    entityBuilder.Property(x => x.Id).ValueGeneratedOnAdd();
                });

            base.OnModelCreating(modelBuilder);
        }
    }
}