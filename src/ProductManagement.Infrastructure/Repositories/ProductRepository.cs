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

    public Task<ProductRecord> InsertProductAsync(Guid storeId, string sku, string name, string? description, decimal price, string currency, int stockQty)
    {
        throw new NotImplementedException();
    }
}