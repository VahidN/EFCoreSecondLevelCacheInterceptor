using EFCoreSecondLevelCacheInterceptor;
using Issue4SpatialType.Entities;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace Issue4SpatialType;

internal static class Program
{
    private static void Main(string[] args)
    {
        //SqlServerTypes.Utilities.LoadNativeAssemblies(AppContext.BaseDirectory)

        initDb();

        EFServiceProvider.RunInContext(context =>
        {
            var cachedProducts = context.Products.Cacheable().ToList();
            cachedProducts = context.Products.Cacheable().ToList();

            foreach (var product in cachedProducts)
            {
                Console.WriteLine($"{product.Id}, {product.Location}");
            }
        });
    }

    private static void initDb()
        => EFServiceProvider.RunInContext(context =>
        {
            context.Database.Migrate();

            if (!context.Products.Any())
            {
                context.People.Add(new Person
                {
                    Name = "Bill"
                });

                var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

                context.Products.AddRange(new Product
                {
                    Location = geometryFactory.CreatePoint(new Coordinate(x: 27.175015, y: 78.042155))
                }, new Product
                {
                    Location = geometryFactory.CreatePoint(new Coordinate(x: 27.175015, y: 78.042155))
                }, new Product
                {
                    Location = geometryFactory.CreatePoint(new Coordinate(x: 27.175015, y: 78.042155))
                });

                context.SaveChanges();
            }
        });
}