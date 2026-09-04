using Dapper;
using Microsoft.Data.SqlClient;
using ProductManagement.Domain.Abstractions;
using ProductManagement.Domain.Models;

namespace ProductManagement.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly string _connectionString;

    public ProductRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<string> GetProductsAsJsonAsync(Guid? storeId)
    {
        using var connection = new SqlConnection(_connectionString);

        var result = await connection.QuerySingleAsync<string>(
            "SELECT dbo.fn_GetProductsAsJson(@StoreId)",
            new { StoreId = storeId });

        return result;
    }

    public async Task<ProductRecord> InsertProductAsync(
        Guid storeId,
        string sku,
        string name,
        string? description,
        decimal price,
        string currency,
        int stockQty)
    {
        using var connection = new SqlConnection(_connectionString);

        var parameters = new
        {
            StoreId = storeId,
            Sku = sku,
            Name = name,
            Description = description,
            Price = price,
            Currency = currency,
            StockQty = stockQty
        };

        var result = await connection.QuerySingleAsync<ProductRecord>(
            "EXEC dbo.sp_InsertProduct @StoreId, @Sku, @Name, @Description, @Price, @Currency, @StockQty",
            parameters);

        return result;
    }

    public Task<ProductRecord?> UpdateProductAsync(Guid productId, string name, string? description, decimal price, string currency, int stockQty, bool isActive)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteProductAsync(Guid productId)
    {
        throw new NotImplementedException();
    }

    public Task<ProductRecord?> GetProductByIdAsync(Guid productId)
    {
        throw new NotImplementedException();
    }
}