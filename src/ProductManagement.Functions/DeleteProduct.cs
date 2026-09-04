using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ProductManagement.Application.Abstractions;

namespace ProductManagement.Functions;

public class DeleteProduct
{
    private readonly IProductsService _productsService;
    private readonly ILogger<DeleteProduct> _logger;

    public DeleteProduct(IProductsService productsService, ILogger<DeleteProduct> logger)
    {
        _productsService = productsService;
        _logger = logger;
    }

    [Function("DeleteProduct")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "products/item/{id}")] HttpRequestData req,
        string id)
    {
        if (!Guid.TryParse(id, out var productId))
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteAsJsonAsync(new { error = "Invalid 'id' format. Expected a GUID." });
            return badRequest;
        }

        var deleted = await _productsService.DeleteProductAsync(productId);

        return req.CreateResponse(deleted ? HttpStatusCode.NoContent : HttpStatusCode.NotFound);
    }
}