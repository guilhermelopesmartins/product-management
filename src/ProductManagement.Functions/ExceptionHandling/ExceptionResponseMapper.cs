using System.Net;
using ProductManagement.Domain.Exceptions;

namespace ProductManagement.Functions.ExceptionHandling;

public static class ExceptionResponseMapper
{
    public static (HttpStatusCode StatusCode, string Title, string Detail) Map(Exception exception) => exception switch
    {
        StoreNotFoundException => (HttpStatusCode.NotFound, "Store not found", exception.Message),
        _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred",
            "An unexpected error occurred. Please try again later.")
    };
}
