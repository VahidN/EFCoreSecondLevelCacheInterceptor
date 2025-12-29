namespace Issue12PostgreSql.Entities;

public class BlogOption
{
    public bool IsActive { get; set; }

    public int NumberOfTimesUsed { get; set; }

    public int SortOrder { get; set; }

    public required string Name { get; set; }
}