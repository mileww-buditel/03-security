namespace Demo.Controllers.Cors;

using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

using static Common.Constants;

[ApiController]
[Route("api/[controller]")]
public sealed class WeatherController : ControllerBase
{
    private static readonly IReadOnlyCollection<WeatherForecast> forecasts =
    [
        new(Date: new DateOnly(2026, 5, 1), TemperatureC: 18, Summary: "Mild"),
        new(Date: new DateOnly(2026, 5, 2), TemperatureC: 22, Summary: "Warm"),
        new(Date: new DateOnly(2026, 5, 3), TemperatureC: 15, Summary: "Cloudy"),
    ];

    // Uses whichever global CORS policy is active in Program.cs.
    [HttpGet("all")]
    public ActionResult<IEnumerable<WeatherForecast>> All()
        => this.Ok(forecasts);

    // Always allows cross-origin requests, regardless of the global policy.
    // Useful for truly public endpoints (e.g. a public status page).
    [HttpGet("public")]
    [EnableCors(CorsPolicies.Correct)]
    public ActionResult<IEnumerable<WeatherForecast>> Public()
        => this.Ok(forecasts);

    // Always blocks cross-origin requests, regardless of the global policy.
    // Useful for sensitive internal endpoints that should never be
    // accessible from a browser on another origin.
    [HttpGet("internal")]
    [DisableCors]
    public ActionResult<IEnumerable<WeatherForecast>> Internal()
        => this.Ok(forecasts);
}

public sealed record WeatherForecast(
    DateOnly Date,
    int TemperatureC,
    string Summary);
