namespace Exercise.Controllers.SqlInjection;

using Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Data;

[ApiController]
[Route("api/[controller]")]
public sealed class EmployeesController(
    HrDbContext data) : ControllerBase
{
    // TODO: ensure SQL Injection is not possible (LINQ, parameterized query, etc)
    [HttpGet("by-department")]
    public async Task<ActionResult<IEnumerable<EmployeeDbModel>>> GetByDepartment(
        [FromQuery] string department,
        CancellationToken cancellationToken = default)
    {
        var employees = await data
            .Employees
            .FromSqlRaw($"SELECT * FROM Employees WHERE Department = '{department}'")
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return this.Ok(employees);
    }
}
