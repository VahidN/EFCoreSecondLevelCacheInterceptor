using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
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
        public DbSet<Entity> Entities { get; set; }

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

            foreach (var property in builder.Model.GetEntityTypes()
                         .SelectMany(t => t.GetProperties())
                         .Where(p => p.GetColumnType() == "jsonb"))
            {
                var converterType = typeof(JsonbConvertor<>).MakeGenericType(property.ClrType);
                var converter = (ValueConverter)Activator.CreateInstance(converterType, (object)null);
                property.SetValueConverter(converter);
            }

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

            // Supporting DateOnly
            foreach (var property in builder.Model.GetEntityTypes()
                         .SelectMany(t => t.GetProperties())
                         .Where(p => p.ClrType == typeof(DateOnly)))
            {
                property.SetValueConverter(
                    new ValueConverter<DateOnly, DateTime>(
                        convertToProviderExpression: dateOnly => dateOnly.ToDateTime(new TimeOnly(0, 0)),
                        convertFromProviderExpression: dateTime => DateOnly.FromDateTime(dateTime)
                    ));
            }

            foreach (var property in builder.Model.GetEntityTypes()
                         .SelectMany(t => t.GetProperties())
                         .Where(p => p.ClrType == typeof(DateOnly?)))
            {
                property.SetValueConverter(
                    new ValueConverter<DateOnly?, DateTime>(
                        convertToProviderExpression: dateOnly => dateOnly.Value.ToDateTime(new TimeOnly(0, 0)),
                        convertFromProviderExpression: dateTime => DateOnly.FromDateTime(dateTime)
                    ));
            }

            // Supporting TimeOnly
            foreach (var property in builder.Model.GetEntityTypes()
                         .SelectMany(t => t.GetProperties())
                         .Where(p => p.ClrType == typeof(TimeOnly)))
            {
                property.SetValueConverter(
                    new ValueConverter<TimeOnly, TimeSpan>(
                        convertToProviderExpression: timeOnly => timeOnly.ToTimeSpan(),
                        convertFromProviderExpression: timeSpan => TimeOnly.FromTimeSpan(timeSpan)
                    ));
            }

            foreach (var property in builder.Model.GetEntityTypes()
                         .SelectMany(t => t.GetProperties())
                         .Where(p => p.ClrType == typeof(TimeOnly?)))
            {
                property.SetValueConverter(
                    new ValueConverter<TimeOnly?, TimeSpan>(
                        convertToProviderExpression: timeOnly => timeOnly.Value.ToTimeSpan(),
                        convertFromProviderExpression: timeSpan => TimeOnly.FromTimeSpan(timeSpan)
                    ));
            }
        }
    }

    public class JsonbConvertor<T> : ValueConverter<T, string>
    {
        private static readonly Expression<Func<T, string>> _convertToProviderExpression = x => Serialize(x);
        private static readonly Expression<Func<string, T>> _convertFromProviderExpression = x => Deserialize(x);

        public JsonbConvertor(ConverterMappingHints mappingHints = null)
            : base(_convertToProviderExpression, _convertFromProviderExpression, mappingHints)
        { }

        private static string Serialize(T x)
        {
            return JsonSerializer.Serialize(x);
        }

        private static T Deserialize(string x)
        {
            return JsonSerializer.Deserialize<T>(x);
        }
    }
}