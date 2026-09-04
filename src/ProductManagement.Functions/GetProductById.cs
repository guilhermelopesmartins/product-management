using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ProductManagement.Application.Abstractions;

namespace ProductManagement.Functions;

public class GetProductById
{
    private readonly IProductsService _productsService;
    private readonly ILogger<GetProductById> _logger;

    public GetProductById(IProductsService productsService, ILogger<GetProductById> logger)
    {
        _productsService = productsService;
        _logger = logger;
    }

    [Function("GetProductById")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products/item/{id}")] HttpRequestData req,
        string id)
    {
        if (!Guid.TryParse(id, out var productId))
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteAsJsonAsync(new { error = "Invalid 'id' format. Expected a GUID." });
            return badRequest;
        }

        var product = await _productsService.GetProductByIdAsync(productId);

        if (product is null)
        {
            var notFound = req.CreateResponse(HttpStatusCode.NotFound);
            return notFound;
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(product);
        return response;
    }
}