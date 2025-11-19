namespace EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Entities;

public class Tag
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public virtual ICollection<TagProduct> TagProducts { get; set; } = new HashSet<TagProduct>();
}