namespace ProductManagement.Application.DTOs.Responses;

public sealed record ProductResponse
{
    public required Guid ProductId { get; init; }
    public required Guid StoreId { get; init; }
    public required string Sku { get; init; }
    public required string Name { get; init; }
    public required decimal Price { get; init; }
    public required string Currency { get; init; }
    public required int StockQty { get; init; }
    public required bool IsActive { get; init; }
}