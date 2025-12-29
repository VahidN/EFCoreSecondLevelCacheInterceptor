using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Issue12PostgreSql.DataLayer;

public class NpgSqlContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var services = new ServiceCollection();

        var basePath = Directory.GetCurrentDirectory();
        Console.WriteLine($"Using `{basePath}` as the ContentRootPath");

        var configuration = new ConfigurationBuilder().SetBasePath(basePath)
            .AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        services.AddSingleton(_ => configuration);

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(configuration[key: "ConnectionStrings:ApplicationDbContextConnection"]);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}