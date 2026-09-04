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
}