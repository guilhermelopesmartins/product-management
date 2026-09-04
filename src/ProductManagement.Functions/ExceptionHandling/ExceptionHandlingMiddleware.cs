using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace ProductManagement.Functions.ExceptionHandling;

public sealed class ExceptionHandlingMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            var logger = context.GetLogger<ExceptionHandlingMiddleware>();
            var (statusCode, title, detail) = ExceptionResponseMapper.Map(exception);

            if (statusCode == HttpStatusCode.InternalServerError)
            {
                logger.LogError(exception, "Unhandled exception while executing {FunctionName}", context.FunctionDefinition.Name);
            }
            else
            {
                logger.LogWarning(exception, "{Title} while executing {FunctionName}", title, context.FunctionDefinition.Name);
            }

            var request = await context.GetHttpRequestDataAsync();
            if (request is null)
            {
                throw;
            }

            var response = request.CreateResponse(statusCode);
            await response.WriteAsJsonAsync(new { error = detail });
            context.GetInvocationResult().Value = response;
        }
    }
}
