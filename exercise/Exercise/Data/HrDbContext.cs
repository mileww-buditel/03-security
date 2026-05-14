namespace Exercise.Data;

using Microsoft.EntityFrameworkCore;
using Models.Data;

public sealed class HrDbContext(
    DbContextOptions<HrDbContext> options) : DbContext(options)
{
    public DbSet<EmployeeDbModel> Employees { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder
            .Entity<EmployeeDbModel>()
            .Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder
            .Entity<EmployeeDbModel>()
            .Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder
            .Entity<EmployeeDbModel>()
            .Property(e => e.Department)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder
            .Entity<EmployeeDbModel>()
            .Property(e => e.JobTitle)
            .IsRequired()
            .HasMaxLength(150);

        modelBuilder
            .Entity<EmployeeDbModel>()
            .Property(e => e.Salary)
            .HasColumnType("decimal(18,2)");
    }
}
