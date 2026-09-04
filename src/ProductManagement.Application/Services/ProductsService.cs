using System.Text.Json;
using ProductManagement.Application.Abstractions;
using ProductManagement.Application.DTOs.Requests;
using ProductManagement.Application.DTOs.Responses;
using ProductManagement.Domain.Abstractions;
using ProductManagement.Domain.Models;

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

    public async Task<ProductResponse> CreateProductAsync(CreateProductRequest request)
    {
        var record = await _repository.InsertProductAsync(
            request.StoreId, request.Sku, request.Name, request.Description,
            request.Price, request.Currency, request.StockQty);

        return MapToResponse(record);
    }

    public async Task<ProductResponse?> GetProductByIdAsync(Guid productId)
    {
        var record = await _repository.GetProductByIdAsync(productId);
        return record is null ? null : MapToResponse(record);
    }

    public async Task<ProductResponse?> UpdateProductAsync(Guid productId, UpdateProductRequest request)
    {
        var record = await _repository.UpdateProductAsync(
            productId, request.Name, request.Description,
            request.Price, request.Currency, request.StockQty, request.IsActive);

        return record is null ? null : MapToResponse(record);
    }

    public Task<bool> DeleteProductAsync(Guid productId) => _repository.DeleteProductAsync(productId);

    private static ProductResponse MapToResponse(ProductRecord record) => new()
    {
        ProductId = record.ProductId,
        StoreId = record.StoreId,
        Sku = record.Sku,
        Name = record.Name,
        Price = record.Price,
        Currency = record.Currency,
        StockQty = record.StockQty,
        IsActive = record.IsActive
    };
}