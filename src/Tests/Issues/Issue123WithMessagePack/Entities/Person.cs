namespace Issue123WithMessagePack.Entities;

public class Person
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public DateTime Date { get; set; }

    public DateTimeOffset DateOffset { get; set; }

    public TimeSpan Span { get; set; }

    public DateOnly DateOnly { get; set; }

    public TimeOnly TimeOnly { get; set; }
}