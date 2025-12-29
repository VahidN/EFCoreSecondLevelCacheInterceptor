#pragma warning disable CA1002,CA2227,MA0016,CA1819
namespace Issue12PostgreSql.Entities;

public class Entity
{
    public long Id { get; set; }

    public int[]? Array { get; set; }

    public List<string>? List { get; set; }
}

#pragma warning restore CA1002,CA2227,MA0016,CA1819