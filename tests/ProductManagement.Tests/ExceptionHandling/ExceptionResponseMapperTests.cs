using System.Net;
using FluentAssertions;
using ProductManagement.Domain.Exceptions;
using ProductManagement.Functions.ExceptionHandling;
using Xunit;

namespace ProductManagement.Tests.ExceptionHandling;

public class ExceptionResponseMapperTests
{
    [Fact]
    public void Map_ShouldReturnNotFound_WhenExceptionIsStoreNotFoundException()
    {
        var exception = new StoreNotFoundException(Guid.NewGuid());

        var (statusCode, title, detail) = ExceptionResponseMapper.Map(exception);

        statusCode.Should().Be(HttpStatusCode.NotFound);
        title.Should().Be("Store not found");
        detail.Should().Be(exception.Message);
    }

    [Fact]
    public void Map_ShouldReturnInternalServerError_WithoutLeakingDetails_WhenExceptionIsUnknown()
    {
        var exception = new InvalidOperationException("some internal secret detail");

        var (statusCode, title, detail) = ExceptionResponseMapper.Map(exception);

        statusCode.Should().Be(HttpStatusCode.InternalServerError);
        title.Should().Be("An unexpected error occurred");
        detail.Should().NotContain("some internal secret detail");
    }
}
