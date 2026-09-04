namespace ProductManagement.Domain.Abstractions;

public interface IProductRepository
{
    Task<string> GetProductsAsJsonAsync(Guid? storeId);
}