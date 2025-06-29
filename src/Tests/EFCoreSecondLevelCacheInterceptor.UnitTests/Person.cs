namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

public class Person
{
    public int Id { get; set; }

    public string Name { get; set; }

    public DateTimeOffset BirthDate { get; set; }

    public decimal Salary { get; set; }

    public Guid UniqueId { get; set; }

    public override bool Equals(object obj)
        => obj is Person person && Id == person.Id && string.Equals(Name, person.Name, StringComparison.Ordinal) &&
           BirthDate == person.BirthDate && Salary == person.Salary && UniqueId == person.UniqueId;

    public override int GetHashCode() => HashCode.Combine(Id, Name, BirthDate, Salary, UniqueId);
}