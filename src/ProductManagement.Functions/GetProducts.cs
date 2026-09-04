using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ProductManagement.Application.Abstractions;
using System.Net;
using System.Web;

namespace ProductManagement.Functions;

public class GetProducts
{
    private readonly IProductsService _productsService;
    private readonly ILogger<GetProducts> _logger;

    public GetProducts(IProductsService productsService, ILogger<GetProducts> logger)
    {
        _productsService = productsService;
        _logger = logger;
    }

    [Function("GetProducts")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products")] HttpRequestData req)
    {
        var query = HttpUtility.ParseQueryString(req.Url.Query);
        var storeIdParam = query["storeId"];

        Guid? storeId = null;
        if (!string.IsNullOrEmpty(storeIdParam))
        {
            if (!Guid.TryParse(storeIdParam, out var parsedStoreId))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteAsJsonAsync(new { error = "Invalid 'storeId' format. Expected a GUID." });
                return badRequest;
            }
            storeId = parsedStoreId;
        }

        _logger.LogInformation("Fetching products. StoreId filter: {StoreId}", storeId);

        var products = await _productsService.GetProductsAsync(storeId);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(products);
        return response;
    }
}