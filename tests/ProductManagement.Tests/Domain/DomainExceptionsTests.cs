using FluentAssertions;
using ProductManagement.Domain.Exceptions;
using Xunit;

namespace ProductManagement.Tests.Domain;

public class DomainExceptionsTests
{
    [Fact]
    public void StoreNotFoundException_ShouldBeADomainException()
    {
        var storeId = Guid.NewGuid();

        var exception = new StoreNotFoundException(storeId);

        exception.Should().BeAssignableTo<DomainException>();
        exception.StoreId.Should().Be(storeId);
        exception.Message.Should().Contain(storeId.ToString());
    }

    [Fact]
    public void DomainException_ShouldBeAnException()
    {
        typeof(DomainException).Should().BeAssignableTo<Exception>();
        typeof(DomainException).IsAbstract.Should().BeTrue();
    }
}
