namespace ProductManagement.Domain.Exceptions;

public sealed class StoreNotFoundException : DomainException
{
    public Guid StoreId { get; }

    public StoreNotFoundException(Guid storeId)
        : base($"Store with id '{storeId}' was not found.")
    {
        StoreId = storeId;
    }
}
