namespace Demo.Controllers.SensitiveDataExposure.Correct;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/broken/[controller]")]
public sealed class UsersController : ControllerBase
{
    private static readonly IReadOnlyCollection<UserDbModel> users =
    [
        new(Id: 1, Username: "alice", Email: "alice@example.com", PasswordHash: "5f4dcc3b5aa765d61d8327deb882cf99", Role: "Admin", IsActive: true, SecurityStamp: "a1b2c3d4e5f6"),
        new(Id: 2, Username: "bob", Email: "bob@example.com", PasswordHash: "d8578edf8458ce06fbc5bb76a58c5ca4", Role: "User", IsActive: true, SecurityStamp: "f6e5d4c3b2a1"),
        new(Id: 3, Username: "charlie", Email: "charlie@example.com", PasswordHash: "96e79218965eb72c92a549dd5a330112", Role: "User", IsActive: false, SecurityStamp: "1a2b3c4d5e6f"),
    ];

    // Safe: maps the entity to a dedicated response model before returning it.
    // The client only receives what it actually needs — id, username, email.
    // PasswordHash, SecurityStamp, Role, and IsActive never leave the server.
    // This pattern is sometimes called a DTO (Data Transfer Object) or
    // a response/view model — the same principle you saw in the Services lecture.
    [HttpGet("{id:int}")]
    public ActionResult<UserResponseModel?> GetById(int id)
    {
        var user = users
            .Select(ToResponseModel)
            .FirstOrDefault(u => u.Id == id);

        if (user is null)
        {
            var error = new { message = $"User {id} was not found." };
            return this.NotFound(error);
        }

        return this.Ok(user);
    }

    private static UserResponseModel ToResponseModel(
        UserDbModel user)
        => new(
            Id: user.Id,
            Username: user.Username,
            Email: user.Email);
}

public sealed record UserDbModel(
    int Id,
    string Username,
    string Email,
    string PasswordHash,
    string Role,
    bool IsActive,
    string SecurityStamp);

public sealed record UserResponseModel(
    int Id,
    string Username,
    string Email);
