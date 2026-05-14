namespace Demo.Controllers.XSS.Correct;

using System.Text;
using Ganss.Xss;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/correct/[controller]")]
public sealed class CommentsController(
    HtmlSanitizer htmlSanitizer) : ControllerBase
{
    private static readonly ICollection<Comment> comments = [];

    // Always prefer to return JSONs from Web API
    [HttpGet("json")]
    public ActionResult<ICollection<Comment>> GetAllJson()
        => this.Ok(comments);

    // If you really want to return HTML for whatever reason, sanitize it using libraries like Ganss.Xss.
    [HttpGet("html")]
    public ContentResult GetAllHtml()
    {
        var html = new StringBuilder();
        html.AppendLine("<ul>");

        foreach (var comment in comments)
        {
            var sanitized = htmlSanitizer.Sanitize(comment.Content);
            html.AppendLine($"<li>{sanitized}</li>");
        }

        html.AppendLine("<ul>");

        return this.Content(
            html.ToString(),
            "text/html");
    }

    // Safe: only stores data. XSS only happens when data is rendered in a browser.
    [HttpPost]
    public NoContentResult Create(
        [FromBody] string content)
    {
        var comment = new Comment(content);
        comments.Add(comment);

        return this.NoContent();
    }
}

public sealed record Comment(
    string Content);
