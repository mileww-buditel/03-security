namespace Demo.Data;

using Microsoft.EntityFrameworkCore;
using Models.Data;

public sealed class ProductsDbContext(
    DbContextOptions<ProductsDbContext> options) : DbContext(options)
{
    public DbSet<ProductDbModel> Products { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder
            .Entity<ProductDbModel>()
            .Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        modelBuilder
            .Entity<ProductDbModel>()
            .Property(p => p.Category)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder
            .Entity<ProductDbModel>()
            .Property(p => p.Price)
            .HasColumnType("decimal(18,2)");
    }
}
