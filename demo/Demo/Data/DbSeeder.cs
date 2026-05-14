namespace Demo.Data;

using Microsoft.EntityFrameworkCore;
using Models.Data;

public static class DbSeeder
{
    public static async Task Seed(
        ProductsDbContext data,
        CancellationToken cancellationToken = default)
    {
        var hasAnyData = await data
            .Products
            .AnyAsync(cancellationToken);

        if (hasAnyData)
        {
            return;
        }

        await SeedProducts(
            data,
            cancellationToken);
    }

    private static async Task SeedProducts(
        ProductsDbContext data,
        CancellationToken cancellationToken)
    {
        var products = new List<ProductDbModel>
        {
            new()
            {
                Name = "Laptop Pro 15",
                Price = 1_299.99m,
                Category = "Electronics"
            },
            new()
            {
                Name = "Wireless Mouse",
                Price = 29.99m,
                Category = "Electronics"
            },
            new()
            {
                Name = "Standing Desk",
                Price = 549.00m,
                Category = "Furniture"
            },
            new()
            {
                Name = "Ergonomic Chair",
                Price = 399.00m,
                Category = "Furniture"
            },
            new()
            {
                Name = "USB-C Hub",
                Price = 49.99m,
                Category = "Electronics"
            },
            new()
            {
                Name = "Notebook A5",
                Price = 4.99m,
                Category = "Stationery"
            },
        };

        data.Products.AddRange(products);
        await data.SaveChangesAsync(cancellationToken);
    }
}
