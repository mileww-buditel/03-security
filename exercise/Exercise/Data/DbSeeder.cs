namespace Exercise.Data;

using Microsoft.EntityFrameworkCore;
using Models.Data;

public static class DbSeeder
{
    public static async Task Seed(
        HrDbContext data,
        CancellationToken cancellationToken = default)
    {
        var hasAnyData = await data
            .Employees
            .AnyAsync(cancellationToken);

        if (hasAnyData)
        {
            return;
        }

        await SeedEmployees(
            data,
            cancellationToken);
    }

    private static async Task SeedEmployees(
        HrDbContext data,
        CancellationToken cancellationToken)
    {
        var employees = new List<EmployeeDbModel>
        {
            new()
            {
                FirstName = "Alice",
                LastName = "Johnson",
                Department = "Engineering",
                JobTitle = "Senior Backend Developer",
                Salary = 5_800.00m,
            },
            new()
            {
                FirstName = "Bob",
                LastName = "Petrov",
                Department = "Engineering",
                JobTitle = "Junior Frontend Developer",
                Salary = 3_200.00m,
            },
            new()
            {
                FirstName = "Clara",
                LastName = "Ivanova",
                Department = "HR",
                JobTitle = "HR Specialist",
                Salary = 3_500.00m,
            },
            new()
            {
                FirstName = "David",
                LastName = "Stoev",
                Department = "Finance",
                JobTitle = "Financial Analyst",
                Salary = 4_200.00m,
            },
            new()
            {
                FirstName = "Elena",
                LastName = "Dimitrova",
                Department = "Finance",
                JobTitle = "Chief Financial Officer",
                Salary = 9_500.00m,
            },
            new()
            {
                FirstName = "Martin",
                LastName = "Georgiev",
                Department = "Engineering",
                JobTitle = "DevOps Engineer",
                Salary = 5_100.00m,
            },
        };

        data.Employees.AddRange(employees);
        await data.SaveChangesAsync(cancellationToken);
    }
}
