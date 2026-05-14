namespace Demo.Controllers.SqlInjection.Broken;

using Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Data;

[ApiController]
[Route("api/broken/[controller]")]
public sealed class ProductsController(
    ProductsDbContext data) : ControllerBase
{
    // Vulnerable: the search term is interpolated directly into the SQL string.
    // EF Core passes the raw string to SQL Server as-is — no parameterization.
    // Try: q = ' OR '1'='1' --
    // This closes the LIKE clause early and appends an always-true condition,
    // returning every row in the table regardless of the search term.
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ProductDbModel>>> Search(
        [FromQuery] string query,
        CancellationToken cancellationToken = default)
    {
        var products = await data
            .Products
            .FromSqlRaw($"SELECT * FROM Products WHERE Name LIKE '%{query}%'")
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return this.Ok(products);
    }
}
