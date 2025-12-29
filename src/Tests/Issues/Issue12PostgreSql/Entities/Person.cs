#pragma warning disable CA1002,CA2227,MA0016,CA1819
using System.ComponentModel.DataAnnotations.Schema;

namespace Issue12PostgreSql.Entities;

public class Person
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public DateTime AddDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public long Points { get; set; }

    public bool IsActive { get; set; }

    public byte ByteValue { get; set; }

    public char CharValue { get; set; }

    public DateTimeOffset DateTimeOffsetValue { get; set; }

    public decimal DecimalValue { get; set; }

    public double DoubleValue { set; get; }

    public float FloatValue { set; get; }

    public Guid GuidValue { set; get; }

    public TimeSpan TimeSpanValue { set; get; }

    public short ShortValue { set; get; }

    public byte[] ByteArrayValue { get; set; } = null!;

    public uint UintValue { set; get; }

    public ulong UlongValue { set; get; }

    public ulong UshortValue { set; get; }

    public ICollection<Address>? Addresses { set; get; }

    public List<Book>? Books { get; set; }

    [Column(TypeName = "jsonb")] public List<BlogOption> OptionDefinitions { get; set; } = new();

    [Column(TypeName = "jsonb")] public CustomFieldDefinitionMetadata? CustomFieldDefinitionMetadata { get; set; }

    public DateOnly Date1 { get; set; }

    public TimeOnly Time1 { get; set; }

    public DateOnly? Date2 { get; set; }

    public TimeOnly? Time2 { get; set; }
}
#pragma warning restore CA1002,CA2227,MA0016,CA1819