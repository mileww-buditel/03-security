namespace Demo.Models.Data;

public sealed class ProductDbModel
{
    public int Id { get; init; }

    public string Name { get; set; } = default!;

    public decimal Price { get; set; }

    public string Category { get; set; } = default!;
}
