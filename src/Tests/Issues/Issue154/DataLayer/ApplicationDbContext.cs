using System;
using System.Linq;
using Issue154.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;

namespace Issue154.DataLayer
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Person> People { get; set; }

       protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.LogTo(message => Console.WriteLine(message));
    }
}