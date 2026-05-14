namespace Demo.Controllers.XSS.Broken;

using System.Text;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/broken/[controller]")]
public sealed class CommentsController : ControllerBase
{
    private static readonly ICollection<Comment> comments = [];

    // Renders raw user input into HTML, allowing XSS if comments contain malicious content.
    [HttpGet]
    public ContentResult GetAll()
    {
        var html = new StringBuilder();
        html.AppendLine("<ul>");

        foreach (var comment in comments)
        {
            html.AppendLine($"<li>{comment.Content}</li>");
        }

        html.AppendLine("</ul>");

        return this.Content(
            html.ToString(),
            "text/html");
    }

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
