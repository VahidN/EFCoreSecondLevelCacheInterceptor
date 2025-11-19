namespace EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Entities;

public class Blog
{
    public int BlogId { get; set; }

    public required string Url { get; set; }

    public virtual ICollection<Post> Posts { get; set; } = new HashSet<Post>();
}