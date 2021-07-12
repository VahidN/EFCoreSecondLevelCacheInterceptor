using System;
using System.Linq;
using Issue12MySQL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;

namespace Issue12MySQL.DataLayer
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

            // MySQL does not support DateTimeOffset
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

            // To solve: Unable to cast object of type 'System.Char' to type 'System.Int32'.
            foreach (var property in builder.Model.GetEntityTypes()
                                                .SelectMany(t => t.GetProperties())
                                                .Where(p => p.ClrType == typeof(char)))
            {
                property.SetValueConverter(
                    new ValueConverter<char, int>(
                        convertToProviderExpression: charValue => charValue,
                        convertFromProviderExpression: intValue => (char)intValue
                    ));
            }

            //To solve: Unable to cast object of type 'System.UInt32' to type 'System.Int32'
            foreach (var property in builder.Model.GetEntityTypes()
                                                .SelectMany(t => t.GetProperties())
                                                .Where(p => p.ClrType == typeof(uint)))
            {
                property.SetValueConverter(
                    new ValueConverter<uint, int>(
                        convertToProviderExpression: uintValue => (int)uintValue,
                        convertFromProviderExpression: intValue => (uint)intValue
                    ));
            }

            //To solve: Unable to cast object of type 'System.UInt64' to type 'System.Int64'
            foreach (var property in builder.Model.GetEntityTypes()
                                                .SelectMany(t => t.GetProperties())
                                                .Where(p => p.ClrType == typeof(ulong)))
            {
                property.SetValueConverter(
                    new ValueConverter<ulong, long>(
                        convertToProviderExpression: ulongValue => (long)ulongValue,
                        convertFromProviderExpression: longValue => (ulong)longValue
                    ));
            }

        }
    }
}