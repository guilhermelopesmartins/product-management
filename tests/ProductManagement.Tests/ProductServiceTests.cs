using FluentAssertions;
using Moq;
using ProductManagement.Application;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.DTOs.Requests;
using ProductManagement.Application.Services;
using ProductManagement.Domain.Abstractions;
using ProductManagement.Domain.Models;
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

    [Fact]
    public async Task CreateProductAsync_ShouldReturnCreatedProduct_WhenRequestIsValid()
    {
        // Arrange
        var mockRepository = new Mock<IProductRepository>();
        var storeId = Guid.NewGuid();

        var expectedRecord = new ProductRecord
        {
            ProductId = Guid.NewGuid(),
            StoreId = storeId,
            Sku = "TEST-002",
            Name = "Novo Produto",
            Description = null,
            Price = 29.90m,
            Currency = "BRL",
            StockQty = 5,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        mockRepository
            .Setup(r => r.InsertProductAsync(storeId, "TEST-002", "Novo Produto", null, 29.90m, "BRL", 5))
            .ReturnsAsync(expectedRecord);

        var sut = new ProductsService(mockRepository.Object);

        var request = new CreateProductRequest
        {
            StoreId = storeId,
            Sku = "TEST-002",
            Name = "Novo Produto",
            Price = 29.90m,
            Currency = "BRL",
            StockQty = 5
        };

        // Act
        var result = await sut.CreateProductAsync(request);

        // Assert
        result.Sku.Should().Be("TEST-002");
        result.Price.Should().Be(29.90m);
        result.StockQty.Should().Be(5);
    }
}