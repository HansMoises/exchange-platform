using MediatR;
using Microsoft.Extensions.Logging;

namespace ExchangePlatform.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var nombre = typeof(TRequest).Name;

        _logger.LogInformation("Ejecutando: {Nombre}", nombre);

        var response = await next();

        _logger.LogInformation("Completado: {Nombre}", nombre);

        return response;
    }
}