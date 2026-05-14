namespace Demo.Controllers.SqlInjection.Correct;

using Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Models.Data;

[ApiController]
[Route("api/correct/[controller]")]
public sealed class ProductsController(
    ProductsDbContext data) : ControllerBase
{
    // Option 1: LINQ.
    // Always prefer LINQ since it's the most readable, straightforward and recognisable way
    // to query data in the modern .NET. If you don't write raw SQL, injection isn't possible to happen.
    [HttpGet("search/linq")]
    public async Task<ActionResult<IEnumerable<ProductDbModel>>> SearchWithLinq(
        [FromQuery] string query,
        CancellationToken cancellationToken = default)
    {
        var products = await data
            .Products
            .AsNoTracking()
            .Where(p => p.Name.Contains(query))
            .ToListAsync(cancellationToken);

        return this.Ok(products);
    }

    // Option 2: DbSet<T>.FromSqlInterpolated().
    // EF Core intercepts the FormattableString and extracts each hole as a SqlParameter.
    // The value never touches the SQL string itself.
    [HttpGet("search/interpolated")]
    public async Task<ActionResult<IEnumerable<ProductDbModel>>> SearchWithInterpolated(
        [FromQuery] string query,
        CancellationToken cancellationToken = default)
    {
        var products = await data
            .Products
            .FromSqlInterpolated($"SELECT * FROM Products WHERE Name LIKE '%{query}%'")
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return this.Ok(products);
    }

    // Option 3: DbSet<T>.FromSqlRaw().
    // The value is passed separately and never concatenated into the query string.
    // Use this when you need full control over the SQL (e.g. complex queries, hints).
    [HttpGet("search/raw")]
    public async Task<ActionResult<IEnumerable<ProductDbModel>>> SearchWithRaw(
        [FromQuery] string query,
        CancellationToken cancellationToken = default)
    {
        var sqlParameter = new SqlParameter(
            "@searchTerm",
            $"%{query}%");

        var products = await data
            .Products
            .FromSqlRaw(
                "SELECT * FROM Products WHERE Name LIKE @searchTerm",
                sqlParameter)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return this.Ok(products);
    }
}
