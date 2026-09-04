using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ProductManagement.Application.Abstractions;
using ProductManagement.Application.DTOs.Requests;

namespace ProductManagement.Functions;

public class UpdateProduct
{
    private readonly IProductsService _productsService;
    private readonly ILogger<UpdateProduct> _logger;

    public UpdateProduct(IProductsService productsService, ILogger<UpdateProduct> logger)
    {
        _productsService = productsService;
        _logger = logger;
    }

    [Function("UpdateProduct")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "products/item/{id}")] HttpRequestData req,
        string id)
    {
        if (!Guid.TryParse(id, out var productId))
        {
            var badRequestId = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestId.WriteAsJsonAsync(new { error = "Invalid 'id' format. Expected a GUID." });
            return badRequestId;
        }

        UpdateProductRequest? request;
        try
        {
            request = await req.ReadFromJsonAsync<UpdateProductRequest>();
        }
        catch (JsonException)
        {
            var invalidJson = req.CreateResponse(HttpStatusCode.BadRequest);
            await invalidJson.WriteAsJsonAsync(new { error = "Invalid JSON body." });
            return invalidJson;
        }

        if (request is null || string.IsNullOrWhiteSpace(request.Name) || request.Price <= 0)
        {
            var invalidData = req.CreateResponse(HttpStatusCode.BadRequest);
            await invalidData.WriteAsJsonAsync(new { error = "Name is required and Price must be greater than zero." });
            return invalidData;
        }

        var updated = await _productsService.UpdateProductAsync(productId, request);

        if (updated is null)
        {
            var notFound = req.CreateResponse(HttpStatusCode.NotFound);
            return notFound;
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(updated);
        return response;
    }
}