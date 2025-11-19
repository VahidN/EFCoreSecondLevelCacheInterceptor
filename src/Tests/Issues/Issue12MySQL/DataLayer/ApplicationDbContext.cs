using Issue12MySQL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Issue12MySQL.DataLayer;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Person> People { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // MySQL does not support DateTimeOffset
        foreach (var property in modelBuilder.Model.GetEntityTypes()
                     .SelectMany(t => t.GetProperties())
                     .Where(p => p.ClrType == typeof(DateTimeOffset)))
        {
            property.SetValueConverter(new ValueConverter<DateTimeOffset, DateTime>(
                dateTimeOffset => dateTimeOffset.UtcDateTime, dateTime => new DateTimeOffset(dateTime)));
        }

        foreach (var property in modelBuilder.Model.GetEntityTypes()
                     .SelectMany(t => t.GetProperties())
                     .Where(p => p.ClrType == typeof(DateTimeOffset?)))
        {
            property.SetValueConverter(new ValueConverter<DateTimeOffset?, DateTime>(
                dateTimeOffset => dateTimeOffset == null ? DateTime.MinValue : dateTimeOffset.Value.UtcDateTime,
                dateTime => new DateTimeOffset(dateTime)));
        }

        // To solve: Unable to cast object of type 'System.Char' to type 'System.Int32'.
        foreach (var property in modelBuilder.Model.GetEntityTypes()
                     .SelectMany(t => t.GetProperties())
                     .Where(p => p.ClrType == typeof(char)))
        {
            property.SetValueConverter(
                new ValueConverter<char, int>(charValue => charValue, intValue => (char)intValue));
        }

        //To solve: Unable to cast object of type 'System.UInt32' to type 'System.Int32'
        foreach (var property in modelBuilder.Model.GetEntityTypes()
                     .SelectMany(t => t.GetProperties())
                     .Where(p => p.ClrType == typeof(uint)))
        {
            property.SetValueConverter(new ValueConverter<uint, int>(uintValue => (int)uintValue,
                intValue => (uint)intValue));
        }

        //To solve: Unable to cast object of type 'System.UInt64' to type 'System.Int64'
        foreach (var property in modelBuilder.Model.GetEntityTypes()
                     .SelectMany(t => t.GetProperties())
                     .Where(p => p.ClrType == typeof(ulong)))
        {
            property.SetValueConverter(new ValueConverter<ulong, long>(ulongValue => (long)ulongValue,
                longValue => (ulong)longValue));
        }
    }
}