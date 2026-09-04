using FluentAssertions;
using Moq;
using ProductManagement.Application;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.DTOs.Requests;
using ProductManagement.Application.Services;
using ProductManagement.Domain.Abstractions;
using ProductManagement.Domain.Exceptions;
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

    [Fact]
    public async Task CreateProductAsync_ShouldPropagateStoreNotFoundException_WhenRepositoryThrowsIt()
    {
        // Arrange
        var mockRepository = new Mock<IProductRepository>();
        var storeId = Guid.NewGuid();

        mockRepository
            .Setup(r => r.InsertProductAsync(storeId, "TEST-003", "Produto Orfao", null, 10.00m, "BRL", 1))
            .ThrowsAsync(new StoreNotFoundException(storeId));

        var sut = new ProductsService(mockRepository.Object);

        var request = new CreateProductRequest
        {
            StoreId = storeId,
            Sku = "TEST-003",
            Name = "Produto Orfao",
            Price = 10.00m,
            Currency = "BRL",
            StockQty = 1
        };

        // Act
        var act = () => sut.CreateProductAsync(request);

        // Assert
        await act.Should().ThrowAsync<StoreNotFoundException>()
            .Where(ex => ex.StoreId == storeId);
    }

    [Fact]
    public async Task GetProductByIdAsync_ShouldReturnProduct_WhenProductExists()
    {
        var mockRepository = new Mock<IProductRepository>();
        var productId = Guid.NewGuid();
        var record = new ProductRecord
        {
            ProductId = productId,
            StoreId = Guid.NewGuid(),
            Sku = "TEST-004",
            Name = "Produto Existente",
            Price = 15.00m,
            Currency = "BRL",
            StockQty = 3,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        mockRepository.Setup(r => r.GetProductByIdAsync(productId)).ReturnsAsync(record);

        var sut = new ProductsService(mockRepository.Object);
        var result = await sut.GetProductByIdAsync(productId);

        result.Should().NotBeNull();
        result!.Sku.Should().Be("TEST-004");
    }

    [Fact]
    public async Task GetProductByIdAsync_ShouldReturnNull_WhenProductDoesNotExist()
    {
        var mockRepository = new Mock<IProductRepository>();
        var productId = Guid.NewGuid();
        mockRepository.Setup(r => r.GetProductByIdAsync(productId)).ReturnsAsync((ProductRecord?)null);

        var sut = new ProductsService(mockRepository.Object);
        var result = await sut.GetProductByIdAsync(productId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldReturnUpdatedProduct_WhenProductExists()
    {
        var mockRepository = new Mock<IProductRepository>();
        var productId = Guid.NewGuid();
        var record = new ProductRecord
        {
            ProductId = productId,
            StoreId = Guid.NewGuid(),
            Sku = "TEST-005",
            Name = "Produto Atualizado",
            Price = 25.00m,
            Currency = "BRL",
            StockQty = 8,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        mockRepository
            .Setup(r => r.UpdateProductAsync(productId, "Produto Atualizado", null, 25.00m, "BRL", 8, true))
            .ReturnsAsync(record);

        var sut = new ProductsService(mockRepository.Object);
        var request = new UpdateProductRequest
        {
            Name = "Produto Atualizado",
            Price = 25.00m,
            Currency = "BRL",
            StockQty = 8,
            IsActive = true
        };

        var result = await sut.UpdateProductAsync(productId, request);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Produto Atualizado");
        result.StockQty.Should().Be(8);
    }

    [Fact]
    public async Task DeleteProductAsync_ShouldReturnTrue_WhenProductWasDeleted()
    {
        var mockRepository = new Mock<IProductRepository>();
        var productId = Guid.NewGuid();
        mockRepository.Setup(r => r.DeleteProductAsync(productId)).ReturnsAsync(true);

        var sut = new ProductsService(mockRepository.Object);
        var result = await sut.DeleteProductAsync(productId);

        result.Should().BeTrue();
    }
}