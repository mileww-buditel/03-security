namespace Exercise.Controllers.Cors;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public sealed class BooksController : ControllerBase
{
    private static readonly IReadOnlyCollection<Book> books =
    [
        new(Id: 1, Title: "Clean Code", Author: "Robert C. Martin", Genre: "Programming", PublishedYear: 2_008),
        new(Id: 2, Title: "The Pragmatic Programmer", Author: "Andrew Hunt", Genre: "Programming", PublishedYear: 1_999),
        new(Id: 3, Title: "Domain-Driven Design", Author: "Eric Evans", Genre: "Architecture", PublishedYear: 2_003),
    ];

    // Should use whichever global CORS policy is active in Program.cs.
    [HttpGet("all")]
    public ActionResult<IEnumerable<Book>> All()
        => this.Ok(books);

    // Should always allow cross-origin requests, regardless of the global policy.
    [HttpGet("public")]
    public ActionResult<IEnumerable<Book>> Public()
        => this.Ok(books);

    // Should always block cross-origin requests, regardless of the global policy.
    [HttpGet("internal")]
    public ActionResult<IEnumerable<Book>> Internal()
        => this.Ok(books);
}

public sealed record Book(
    int Id,
    string Title,
    string Author,
    string Genre,
    int PublishedYear);
