using ProductManagement.Application.DTOs;
using ProductManagement.Application.DTOs.Requests;
using ProductManagement.Application.DTOs.Responses;

namespace ProductManagement.Application.Abstractions;

public interface IProductsService
{
    Task<List<ProductResponse>> GetProductsAsync(Guid? storeId);
    Task<ProductResponse> CreateProductAsync(CreateProductRequest request);
    Task<ProductResponse?> GetProductByIdAsync(Guid productId);
    Task<ProductResponse?> UpdateProductAsync(Guid productId, UpdateProductRequest request);
    Task<bool> DeleteProductAsync(Guid productId);
}