namespace ProductManagement.Application;

public interface IProductRepository
{
    Task<string> GetProductsAsJsonAsync(Guid? storeId);
}