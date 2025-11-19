namespace EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Entities;

public class Product
{
    public int ProductId { get; set; }

    public required string ProductNumber { get; set; }

    public required string ProductName { get; set; }

    public required string Notes { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<TagProduct> TagProducts { get; set; } = new HashSet<TagProduct>();

    public virtual User? User { get; set; }

    public int UserId { get; set; }
}