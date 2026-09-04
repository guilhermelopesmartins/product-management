using Moq;
using FluentAssertions;
using ProductManagement.Application;
using ProductManagement.Application.DTOs;
using ProductManagement.Domain.Abstractions;
using ProductManagement.Application.Services;
using Xunit;

namespace ProductManagement.Tests;

public class ProductServiceTests
{
    [Fact]
    public async Task GetProductsAsync_ShouldReturnProductList_WhenRepositoryReturnsValidJson()
    {
        // Arrange
        var mockRepository = new Mock<IProductRepository>();
        var json = """
        [
            { "ProductId": "11111111-1111-1111-1111-111111111111", "StoreId": "99999999-9999-9999-9999-999999999999", "Sku": "TEST-001", "Name": "Produto de Teste", "Price": 19.90, "Currency": "BRL", "StockQty": 10, "IsActive": true }
        ]
        """;
        mockRepository.Setup(r => r.GetProductsAsJsonAsync(null)).ReturnsAsync(json);

        var sut = new ProductsService(mockRepository.Object);

        // Act
        var result = await sut.GetProductsAsync(storeId: null);

        // Assert
        result.Should().HaveCount(1);
        result[0].Sku.Should().Be("TEST-001");
        result[0].Price.Should().Be(19.90m);
    }
}