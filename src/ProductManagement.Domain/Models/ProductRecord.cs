namespace ProductManagement.Domain.Models;

public sealed record ProductRecord
{
    public required Guid ProductId { get; init; }
    public required Guid StoreId { get; init; }
    public required string Sku { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required decimal Price { get; init; }
    public required string Currency { get; init; }
    public required int StockQty { get; init; }
    public required bool IsActive { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}