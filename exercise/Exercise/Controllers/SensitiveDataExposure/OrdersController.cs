namespace Exercise.Controllers.SensitiveDataExposure;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public sealed class OrdersController : ControllerBase
{
    private static readonly IReadOnlyCollection<OrderDbModel> orders =
    [
        new(
            Id: 1,
            CustomerId: 301,
            Status: "Delivered",
            InternalStatus: "FRAUD_REVIEW_PASSED",
            TotalAmount: 149.99m,
            CostPrice: 62.40m,
            CardLastFour: "4242",
            CardToken: "tok_live_xK92mNpQrL8vT3wY",
            BillingAddress: "14 Elm Street, Springfield, IL 62701",
            CreatedAt: new DateOnly(2025, 3, 12)),
        new(
            Id: 2,
            CustomerId: 302,
            Status: "Processing",
            InternalStatus: "AWAITING_WAREHOUSE",
            TotalAmount: 39.95m,
            CostPrice: 11.20m,
            CardLastFour: "1337",
            CardToken: "tok_live_aB34cDeFgH56iJkL",
            BillingAddress: "7 Oak Avenue, Portland, OR 97201",
            CreatedAt: new DateOnly(2025, 4, 1)),
        new(
            Id: 3,
            CustomerId: 301,
            Status: "Cancelled",
            InternalStatus: "REFUND_ISSUED",
            TotalAmount: 89.00m,
            CostPrice: 34.75m,
            CardLastFour: "4242",
            CardToken: "tok_live_xK92mNpQrL8vT3wY",
            BillingAddress: "14 Elm Street, Springfield, IL 62701",
            CreatedAt: new DateOnly(2025, 4, 18)),
    ];

    // TODO: Fix sensitive data exposure.
    // - Introduce an OrderResponseModel with only the safe fields:
    //   Id, CustomerId, Status, TotalAmount, CreatedAt
    // - Map OrderDbModel to OrderResponseModel before returning it
    [HttpGet("{id:int}")]
    public ActionResult<OrderDbModel?> GetById(int id)
    {
        var order = orders.FirstOrDefault(o => o.Id == id);
        if (order is null)
        {
            var error = new { message = $"Order {id} was not found." };
            return this.NotFound(error);
        }

        return this.Ok(order);
    }
}

public sealed record OrderDbModel(
    int Id,
    int CustomerId,
    string Status,
    string InternalStatus,
    decimal TotalAmount,
    decimal CostPrice,
    string CardLastFour,
    string CardToken,
    string BillingAddress,
    DateOnly CreatedAt);
