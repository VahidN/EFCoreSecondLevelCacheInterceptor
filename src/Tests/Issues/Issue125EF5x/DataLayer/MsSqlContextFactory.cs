using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;

namespace Issue125EF5x.DataLayer
{
    public class MsSqlContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var services = new ServiceCollection();

            var basePath = Directory.GetCurrentDirectory();
            Console.WriteLine($"Using `{basePath}` as the ContentRootPath");
            var configuration = new ConfigurationBuilder()
                                .SetBasePath(basePath)
                                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                .Build();
            services.AddSingleton(_ => configuration);

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(EFServiceProvider.GetConnectionString(basePath, configuration));
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}