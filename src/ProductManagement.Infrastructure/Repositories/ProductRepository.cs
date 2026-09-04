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

    public async Task<ProductRecord?> GetProductByIdAsync(Guid productId)
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.QuerySingleOrDefaultAsync<ProductRecord>(
            "EXEC dbo.sp_GetProductById @ProductId",
            new { ProductId = productId });
    }

    public async Task<ProductRecord?> UpdateProductAsync(
        Guid productId, string name, string? description,
        decimal price, string currency, int stockQty, bool isActive)
    {
        using var connection = new SqlConnection(_connectionString);

        var parameters = new { ProductId = productId, Name = name, Description = description, Price = price, Currency = currency, StockQty = stockQty, IsActive = isActive };

        return await connection.QuerySingleOrDefaultAsync<ProductRecord>(
            "EXEC dbo.sp_UpdateProduct @ProductId, @Name, @Description, @Price, @Currency, @StockQty, @IsActive",
            parameters);
    }

    public async Task<bool> DeleteProductAsync(Guid productId)
    {
        using var connection = new SqlConnection(_connectionString);

        var deletedCount = await connection.QuerySingleAsync<int>(
            "EXEC dbo.sp_DeleteProduct @ProductId",
            new { ProductId = productId });

        return deletedCount > 0;
    }
}