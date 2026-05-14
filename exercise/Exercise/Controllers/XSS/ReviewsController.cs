namespace Exercise.Controllers.Xss;

using System.Text;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public sealed class ReviewsController : ControllerBase
{
    private static readonly ICollection<BookReview> reviews = [];

    // TODO: ensure XSS attack is not possilbe (do not retutn html or use Ganss.Xss.HtmlSanitizer)
    [HttpGet]
    public ContentResult GetAll()
    {
        var html = new StringBuilder();
        html.AppendLine("<ul>");

        foreach (var review in reviews)
        {
            html.AppendLine($"<li><strong>{review.BookTitle}</strong> — {review.Body}</li>");
        }

        html.AppendLine("</ul>");

        return this.Content(
            html.ToString(),
            "text/html");
    }

    [HttpPost]
    public NoContentResult Create(
        [FromBody] BookReview review)
    {
        reviews.Add(review);
        return this.NoContent();
    }
}

public sealed record BookReview(
    string BookTitle,
    string Body);
