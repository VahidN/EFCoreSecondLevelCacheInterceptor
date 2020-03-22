using System;
using System.Linq;
using Issue9SQLiteInt32.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;

namespace Issue9SQLiteInt32.DataLayer
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Person> People { get; set; }

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

            // SQLite does not support DateTimeOffset
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


            // SQLite does not support TimeSpan
            foreach (var property in builder.Model.GetEntityTypes()
                                                .SelectMany(t => t.GetProperties())
                                                .Where(p => p.ClrType == typeof(TimeSpan)))
            {
                property.SetValueConverter(
                    new ValueConverter<TimeSpan, string>(
                        convertToProviderExpression: timeSpan => timeSpan.ToString(),
                        convertFromProviderExpression: timeSpanString => TimeSpan.Parse(timeSpanString)
                    ));
            }

            foreach (var property in builder.Model.GetEntityTypes()
                                                .SelectMany(t => t.GetProperties())
                                                .Where(p => p.ClrType == typeof(TimeSpan?)))
            {
                property.SetValueConverter(
                    new ValueConverter<TimeSpan?, string>(
                        convertToProviderExpression: timeSpan => timeSpan.Value.ToString(),
                        convertFromProviderExpression: timeSpanString => TimeSpan.Parse(timeSpanString)
                    ));
            }


            // SQLite does not support uint32
            foreach (var property in builder.Model.GetEntityTypes()
                                                .SelectMany(t => t.GetProperties())
                                                .Where(p => p.ClrType == typeof(uint)))
            {
                property.SetValueConverter(
                    new ValueConverter<uint, long>(
                        convertToProviderExpression: uintValue => (long)uintValue,
                        convertFromProviderExpression: longValue => (uint)longValue
                    ));
            }

            foreach (var property in builder.Model.GetEntityTypes()
                                                .SelectMany(t => t.GetProperties())
                                                .Where(p => p.ClrType == typeof(uint?)))
            {
                property.SetValueConverter(
                    new ValueConverter<uint?, long>(
                        convertToProviderExpression: uintValue => (long)uintValue.Value,
                        convertFromProviderExpression: longValue => (uint)longValue
                    ));
            }


            // SQLite does not support ulong
            foreach (var property in builder.Model.GetEntityTypes()
                                                .SelectMany(t => t.GetProperties())
                                                .Where(p => p.ClrType == typeof(ulong)))
            {
                property.SetValueConverter(
                    new ValueConverter<ulong, long>(
                        convertToProviderExpression: ulongValue => (long)ulongValue,
                        convertFromProviderExpression: longValue => (uint)longValue
                    ));
            }

            foreach (var property in builder.Model.GetEntityTypes()
                                                .SelectMany(t => t.GetProperties())
                                                .Where(p => p.ClrType == typeof(ulong?)))
            {
                property.SetValueConverter(
                    new ValueConverter<ulong?, long>(
                        convertToProviderExpression: ulongValue => (long)ulongValue.Value,
                        convertFromProviderExpression: longValue => (uint)longValue
                    ));
            }

        }
    }
}