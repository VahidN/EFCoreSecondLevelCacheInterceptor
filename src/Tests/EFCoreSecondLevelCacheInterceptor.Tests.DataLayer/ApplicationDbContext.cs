using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace EFCoreSecondLevelCacheInterceptor.Tests.DataLayer;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        // Entity Framework 7 will by default only creates a transaction "WhenNeeded"
        // This will break the cache invalidation process.
        // To fix it add this line
        // Database.AutoTransactionBehavior = AutoTransactionBehavior.Always
    }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<TagProduct> TagProducts { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Blog> Blogs { get; set; }

    public virtual DbSet<DateType> DateTypes { get; set; }

    public virtual DbSet<EngineVersion> EngineVersions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasData(new Blog
            {
                BlogId = 1,
                Url = "https://site1.com"
            });

            entity.HasData(new Blog
            {
                BlogId = 2,
                Url = "https://site2.com"
            });
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasIndex(e => e.UserId);

            entity.HasOne(d => d.User).WithMany(p => p.Posts).HasForeignKey(d => d.UserId);

            entity.HasDiscriminator<string>(name: "post_type")
                .HasValue<Post>(value: "post_base")
                .HasValue<Page>(value: "post_page");

            entity.HasData(new Post
            {
                Id = 1,
                Title = "Post1",
                UserId = 1,
                BlogId = 1,
                User = null
            }, new Post
            {
                Id = 2,
                Title = "Post2",
                UserId = 1,
                BlogId = 1
            });
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId);

            entity.HasIndex(e => e.ProductName).IsUnique();

            entity.HasIndex(e => e.UserId);

            entity.Property(e => e.ProductName).IsRequired().HasMaxLength(maxLength: 50);

            entity.Property(e => e.ProductNumber).IsRequired().HasMaxLength(maxLength: 30);

            entity.HasOne(d => d.User).WithMany(p => p.Products).HasForeignKey(d => d.UserId);

            entity.HasData(new Product
            {
                ProductId = 1,
                ProductName = "Product4",
                IsActive = false,
                Notes = "Notes ...",
                ProductNumber = "004",
                UserId = 1
            }, new Product
            {
                ProductId = 2,
                ProductName = "Product1",
                IsActive = true,
                Notes = "Notes ...",
                ProductNumber = "001",
                UserId = 1
            }, new Product
            {
                ProductId = 3,
                ProductName = "Product2",
                IsActive = true,
                Notes = "Notes ...",
                ProductNumber = "002",
                UserId = 1
            }, new Product
            {
                ProductId = 4,
                ProductName = "Product3",
                IsActive = true,
                Notes = "Notes ...",
                ProductNumber = "003",
                UserId = 1
            });
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasData(new Tag
            {
                Id = 1,
                Name = "Tag4"
            }, new Tag
            {
                Id = 2,
                Name = "Tag1"
            }, new Tag
            {
                Id = 3,
                Name = "Tag2"
            }, new Tag
            {
                Id = 4,
                Name = "Tag3"
            });
        });

        modelBuilder.Entity<TagProduct>(entity =>
        {
            entity.HasKey(e => new
            {
                e.TagId,
                e.ProductProductId
            });

            entity.HasIndex(e => e.ProductProductId);

            entity.HasIndex(e => e.TagId);

            entity.Property(e => e.TagId);

            entity.Property(e => e.ProductProductId);

            entity.HasOne(d => d.Product).WithMany(p => p.TagProducts).HasForeignKey(d => d.ProductProductId);

            entity.HasOne(d => d.Tag).WithMany(p => p.TagProducts).HasForeignKey(d => d.TagId);

            entity.HasData(new TagProduct
            {
                TagId = 1,
                ProductProductId = 1
            }, new TagProduct
            {
                TagId = 2,
                ProductProductId = 2
            }, new TagProduct
            {
                TagId = 3,
                ProductProductId = 3
            }, new TagProduct
            {
                TagId = 4,
                ProductProductId = 4
            });
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Name).IsRequired();

            entity.HasData(new User
            {
                Id = 1,
                Name = "User1",
                UserStatus = UserStatus.Active,
                AddDate = null,
                UpdateDate = null,
                Points = 1000,
                IsActive = true,
                ByteValue = 1,
                CharValue = 'C',
                DateTimeOffsetValue = null,
                DecimalValue = 1.1M,
                DoubleValue = 1.3,
                FloatValue = 1.2f,
                GuidValue = new Guid(g: "236bbe40-b861-433c-8789-b152a99cfe3e"),
                TimeSpanValue = null,
                ShortValue = 2,
                ByteArrayValue = new byte[]
                {
                    1, 2
                },
                UintValue = 1,
                UlongValue = 1,
                UshortValue = 1,
                ImageData = []
            });
        });

        modelBuilder.Entity<BlogData>().HasNoKey();

        modelBuilder.Entity<EngineVersion>().OwnsOne(p => p.Commercial);
        modelBuilder.Entity<EngineVersion>().OwnsOne(p => p.Retail);
    }
}