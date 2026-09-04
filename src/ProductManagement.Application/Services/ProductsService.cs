using ProductManagement.Application.Abstractions;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.DTOs.Responses;
using ProductManagement.Domain.Abstractions;
using System.Text.Json;

namespace ProductManagement.Application.Services;

public class ProductsService : IProductsService
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