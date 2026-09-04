namespace ProductManagement.Application.DTOs.Requests;

public sealed record CreateProductRequest
{
    public required Guid StoreId { get; init; }
    public required string Sku { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required decimal Price { get; init; }
    public string Currency { get; init; } = "BRL";
    public int StockQty { get; init; } = 0;
}