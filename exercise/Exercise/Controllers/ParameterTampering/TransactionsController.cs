namespace Exercise.Controllers.ParameterTampering;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public sealed class TransactionsController : ControllerBase
{
    private static readonly IReadOnlyCollection<Transaction> transactions =
    [
        new(Id: 1, AccountId: 201, Amount: 1_500.00m, Description: "Salary deposit"),
        new(Id: 2, AccountId: 201, Amount: -200.00m,  Description: "Grocery store"),
        new(Id: 3, AccountId: 202, Amount: -850.00m,  Description: "Rent payment"),
        new(Id: 4, AccountId: 203, Amount: 3_200.00m, Description: "Freelance invoice"),
    ];

    // TODO:
    // - Accept the current user's account id from the request header: X-Account-Id
    // - Return 403 Forbidden if the transaction does not belong to the requesting account
    [HttpGet("{id:int}")]
    public ActionResult<Transaction?> GetById(int id)
    {
        var transaction = transactions.FirstOrDefault(t => t.Id == id);
        if (transaction is null)
        {
            var error = new { message = $"Transaction {id} was not found." };
            return this.NotFound(error);
        }

        return this.Ok(transaction);
    }
}

public sealed record Transaction(
    int Id,
    int AccountId,
    decimal Amount,
    string Description);
