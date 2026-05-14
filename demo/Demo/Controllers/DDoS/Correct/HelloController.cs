namespace Demo.Controllers.DDoS.Correct;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using static Common.Constants;

[ApiController]
[Route("api/correct/[controller]")]
public sealed class HelloController : ControllerBase
{
    // Safe: the rate limiting middleware enforces a fixed window policy.
    // Once the limit is hit, the server returns 429 Too Many Requests
    // and the attacker's requests are rejected before reaching this code.
    [HttpGet]
    [EnableRateLimiting(RateLimiterPolicies.LoginFixedWindow)]
    public ActionResult<Response> Hello()
    {
        var data = new Response("Hello World!");
        return this.Ok(data);
    }
}

public sealed record Response(
    string Message);
