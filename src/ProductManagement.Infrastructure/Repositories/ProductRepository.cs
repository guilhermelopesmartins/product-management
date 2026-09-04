using Dapper;
using Microsoft.Data.SqlClient;
using ProductManagement.Domain.Abstractions;

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
}