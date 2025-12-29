namespace Issue12PostgreSql.Entities;

public class Book
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public Person Person { get; set; } = null!;

    public int PersonId { get; set; }
}