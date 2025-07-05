using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestEase;
using SportManager.Application.Common.Models;


namespace SportManager.API.Middleware;

public class ErrorExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorExceptionMiddleware> _logger;

    public ErrorExceptionMiddleware(RequestDelegate next, ILogger<ErrorExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
#if DEBUG
            Console.WriteLine(ex);
            Console.WriteLine(ex.StackTrace);
#endif
            await HandleExceptionAsync(httpContext, ex, _logger);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception, ILogger logger)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            }
        };

        if (exception is UnauthorizedAccessException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }

        logger?.LogWarning("Handling Exception Middleware: {Exception}", exception);
        var (statusCode, modelResult) = ErrorExceptionMiddlewareHandler.Handle(exception);
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsync(
            JsonConvert.SerializeObject(modelResult, jsonSettings));
    }
}

public static class ErrorExceptionMiddlewareHandler
{
    public static (int statusCode, ResultModel resultModel) Handle(
        Exception exception)
    {
        switch (exception)
        {
            case FluentValidation.ValidationException validationException:
                var validationErrorModel = ResultModel
                    .Failure(validationException.Errors.GroupBy(x => x.PropertyName)
                        .ToDictionary(x => x.Key, x => x.Select(y => y.ErrorMessage).ToList()));
                return (StatusCodes.Status400BadRequest, validationErrorModel);

            case ApiException restEaseException:
                return (StatusCodes.Status400BadRequest,
                    !string.IsNullOrEmpty(restEaseException.Content) ?
                    JsonConvert.DeserializeObject<ResultModel>(restEaseException.Content) ?? ResultModel.Failure(restEaseException.Content) :
                    ResultModel.Failure(restEaseException.Content));

            default:
                {
                    return (StatusCodes.Status500InternalServerError, ResultModel.Failure($"{exception.Message}-{exception!.InnerException?.Message}"));
                }
        }
    }
}
