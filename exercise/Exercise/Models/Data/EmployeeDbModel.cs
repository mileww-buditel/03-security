namespace Exercise.Models.Data;

public sealed class EmployeeDbModel
{
    public int Id { get; init; }

    public string FirstName { get; init; } = null!;

    public string LastName { get; init; } = null!;

    public string Department { get; init; } = null!;

    public string JobTitle { get; init; } = null!;

    public decimal Salary { get; init; }
}
