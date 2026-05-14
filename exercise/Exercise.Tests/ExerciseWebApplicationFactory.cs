namespace Exercise.Tests;

using Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public sealed class ExerciseWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        this.connection.Open();

        builder
            .UseEnvironment("Testing")
            .ConfigureLogging(static logging => logging.ClearProviders())
            .ConfigureServices(services =>
            {
                services
                    .Where(static descriptor =>
                        descriptor.ServiceType.FullName is not null &&
                        (descriptor.ServiceType == typeof(HrDbContext) ||
                        descriptor.ServiceType.FullName.Contains("DbContext") ||
                        descriptor.ServiceType.FullName.Contains("SqlServer") ||
                        descriptor.ServiceType.FullName.Contains("Relational") ||
                        (descriptor.ServiceType.IsGenericType &&
                        descriptor
                            .ServiceType
                            .GetGenericArguments()
                            .Any(static arg => arg == typeof(HrDbContext)))))
                    .ToList()
                    .ForEach(descriptor => services.Remove(descriptor));

                services.AddDbContext<HrDbContext>(options =>
                    options
                        .UseSqlite(this.connection)
                        .ConfigureWarnings(static warning =>
                            warning.Ignore(RelationalEventId.PendingModelChangesWarning)));

                var serviceProvider = services.BuildServiceProvider();
                using var scope = serviceProvider.CreateScope();

                var data = scope
                    .ServiceProvider
                    .GetRequiredService<HrDbContext>();

                data.Database.EnsureCreated();

                data
                    .Employees
                    .AddRange(
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
                        });

                data.SaveChanges();
            });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        this.connection.Dispose();
    }
}
