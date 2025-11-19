namespace EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Entities;

public class User
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public UserStatus UserStatus { get; set; }

#pragma warning disable CA1819
    public required byte[] ImageData { get; set; }
#pragma warning restore CA1819

    public DateTime? AddDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public long Points { get; set; }

    public bool IsActive { get; set; }

    public byte ByteValue { get; set; }

    public char CharValue { get; set; }

    public DateTimeOffset? DateTimeOffsetValue { get; set; }

    public decimal DecimalValue { get; set; }

    public double DoubleValue { set; get; }

    public float FloatValue { set; get; }

    public Guid GuidValue { set; get; }

    public TimeSpan? TimeSpanValue { set; get; }

    public short ShortValue { set; get; }

#pragma warning disable CA1819
    public required byte[] ByteArrayValue { get; set; }
#pragma warning restore CA1819

    public uint UintValue { set; get; }

    public ulong UlongValue { set; get; }

    public ulong UshortValue { set; get; }

    public virtual ICollection<Post> Posts { get; set; } = new HashSet<Post>();

    public virtual ICollection<Product> Products { get; set; } = new HashSet<Product>();
}