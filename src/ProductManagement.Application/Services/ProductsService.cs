using ProductManagement.Application.DTOs;
using System.Text.Json;

namespace ProductManagement.Application;

public class ProductsService
{
    private readonly IProductRepository _repository;

    public ProductsService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ProductResponse>> GetProductsAsync(Guid? storeId)
    {
        var json = await _repository.GetProductsAsJsonAsync(storeId);

        var products = JsonSerializer.Deserialize<List<ProductResponse>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return products ?? [];
    }
}