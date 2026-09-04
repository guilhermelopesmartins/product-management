using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ProductManagement.Application.Abstractions;
using ProductManagement.Application.DTOs.Requests;

namespace ProductManagement.Functions;

public class CreateProduct
{
    private readonly IProductsService _productsService;
    private readonly ILogger<CreateProduct> _logger;

    public CreateProduct(IProductsService productsService, ILogger<CreateProduct> logger)
    {
        _productsService = productsService;
        _logger = logger;
    }

    [Function("CreateProduct")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "products")] HttpRequestData req)
    {
        CreateProductRequest? request;
        try
        {
            request = await req.ReadFromJsonAsync<CreateProductRequest>();
        }
        catch (JsonException)
        {
            var invalidJson = req.CreateResponse(HttpStatusCode.BadRequest);
            await invalidJson.WriteAsJsonAsync(new { error = "Invalid JSON body." });
            return invalidJson;
        }

        if (request is null)
        {
            var emptyBody = req.CreateResponse(HttpStatusCode.BadRequest);
            await emptyBody.WriteAsJsonAsync(new { error = "Request body is required." });
            return emptyBody;
        }

        if (string.IsNullOrWhiteSpace(request.Sku) || string.IsNullOrWhiteSpace(request.Name) || request.Price <= 0)
        {
            var invalidData = req.CreateResponse(HttpStatusCode.BadRequest);
            await invalidData.WriteAsJsonAsync(new { error = "Sku, Name are required and Price must be greater than zero." });
            return invalidData;
        }

        _logger.LogInformation("Creating product {Sku} for store {StoreId}", request.Sku, request.StoreId);

        var created = await _productsService.CreateProductAsync(request);

        var response = req.CreateResponse(HttpStatusCode.Created);
        await response.WriteAsJsonAsync(created);
        return response;
    }
}