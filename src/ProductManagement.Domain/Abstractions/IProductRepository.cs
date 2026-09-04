using ProductManagement.Domain.Models;

namespace ProductManagement.Domain.Abstractions;

public interface IProductRepository
{
    Task<string> GetProductsAsJsonAsync(Guid? storeId);
    Task<ProductRecord> InsertProductAsync(
    Guid storeId,
    string sku,
    string name,
    string? description,
    decimal price,
    string currency,
    int stockQty);
    Task<ProductRecord?> GetProductByIdAsync(Guid productId);

    Task<ProductRecord?> UpdateProductAsync(
        Guid productId, string name, string? description,
        decimal price, string currency, int stockQty, bool isActive);

    Task<bool> DeleteProductAsync(Guid productId);
}