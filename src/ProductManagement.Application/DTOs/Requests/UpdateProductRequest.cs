namespace ProductManagement.Application.DTOs.Requests;

public sealed record UpdateProductRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required decimal Price { get; init; }
    public required string Currency { get; init; }
    public required int StockQty { get; init; }
    public required bool IsActive { get; init; }
}