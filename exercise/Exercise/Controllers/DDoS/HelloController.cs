namespace Exercise.Controllers.DDoS;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public sealed class HelloController : ControllerBase
{
    // Should use the Exercise.Common.Constants.RateLimiterPolicies.LoginFixedWindow policy.
    [HttpGet]
    public ActionResult<Response> Hello()
    {
        var data = new Response("Hello World!");
        return this.Ok(data);
    }
}

public sealed record Response(
    string Message);
