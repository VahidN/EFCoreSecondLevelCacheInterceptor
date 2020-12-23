using System;
using System.Linq;
using Issue12PostgreSql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;

namespace Issue12PostgreSql.DataLayer
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Person> People { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Book> Books { get; set; }

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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // It does not support DateTimeOffset
            foreach (var property in builder.Model.GetEntityTypes()
                                                .SelectMany(t => t.GetProperties())
                                                .Where(p => p.ClrType == typeof(DateTimeOffset)))
            {
                property.SetValueConverter(
                    new ValueConverter<DateTimeOffset, DateTime>(
                        convertToProviderExpression: dateTimeOffset => dateTimeOffset.UtcDateTime,
                        convertFromProviderExpression: dateTime => new DateTimeOffset(dateTime)
                    ));
            }

            foreach (var property in builder.Model.GetEntityTypes()
                                                .SelectMany(t => t.GetProperties())
                                                .Where(p => p.ClrType == typeof(DateTimeOffset?)))
            {
                property.SetValueConverter(
                    new ValueConverter<DateTimeOffset?, DateTime>(
                        convertToProviderExpression: dateTimeOffset => dateTimeOffset.Value.UtcDateTime,
                        convertFromProviderExpression: dateTime => new DateTimeOffset(dateTime)
                    ));
            }

        }
    }
}