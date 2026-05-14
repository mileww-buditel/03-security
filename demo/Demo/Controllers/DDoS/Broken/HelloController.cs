namespace Demo.Controllers.DDoS.Broken;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/broken/[controller]")]
public sealed class HelloController : ControllerBase
{
    // Vulnerable: no rate limiting applied.
    // An attacker can hammer this endpoint with thousands of requests —
    // brute-forcing credentials or simply exhausting server resources.
    [HttpGet]
    public ActionResult<Response> Hello()
    {
        var data = new Response("Hello World!");
        return this.Ok(data);
    }
}

public sealed record Response(
    string Message);
