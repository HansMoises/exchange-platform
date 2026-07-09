using ExchangePlatform.Application.Common.Exceptions;
using ExchangePlatform.Application.Common.Models;
using ExchangePlatform.Domain.Exceptions;
using System.Text.Json;

namespace ExchangePlatform.API.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await ManejarExcepcionAsync(context, ex);
        }
    }

    private async Task ManejarExcepcionAsync(HttpContext context, Exception ex)
    {
        int statusCode;
        ApiResponse<object> response;

        switch (ex)
        {
            case ValidationException ve:
                statusCode = StatusCodes.Status422UnprocessableEntity;
                response = ApiResponse<object>.Fail(
                    "Error de validación.", ve.Errors);
                break;

            case NotFoundException ne:
                statusCode = StatusCodes.Status404NotFound;
                response = ApiResponse<object>.Fail(ne.Message);
                break;

            case ConflictException ce:
                statusCode = StatusCodes.Status409Conflict;
                response = ApiResponse<object>.Fail(ce.Message);
                break;

            case ForbiddenException fe:
                statusCode = StatusCodes.Status403Forbidden;
                response = ApiResponse<object>.Fail(fe.Message);
                break;

            case DomainException de:
                statusCode = StatusCodes.Status422UnprocessableEntity;
                response = ApiResponse<object>.Fail(de.Message);
                break;

            default:
                _logger.LogError(ex, "Error no controlado: {Mensaje}", ex.Message);
                statusCode = StatusCodes.Status500InternalServerError;
                response = ApiResponse<object>.Fail(
                    "Ocurrió un error interno. Intenta nuevamente.");
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var json = JsonSerializer.Serialize(response,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        await context.Response.WriteAsync(json);
    }
}