namespace Issue12PostgreSql.Entities;

public class Address
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public Person Person { get; set; } = null!;

    public int PersonId { get; set; }
}