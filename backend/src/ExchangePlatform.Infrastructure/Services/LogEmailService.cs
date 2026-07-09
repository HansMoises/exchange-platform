using ExchangePlatform.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace ExchangePlatform.Infrastructure.Services;

// No hay proveedor de correo configurado (SMTP/SendGrid/etc.) todavia.
// Registra el envio en el log en vez de enviarlo de verdad, para no bloquear
// el flujo de recuperacion de contrasena mientras se define el proveedor real.
// Reemplazar por una implementacion real de IEmailService cuando se decida.
public class LogEmailService : IEmailService
{
    private readonly ILogger<LogEmailService> _logger;

    public LogEmailService(ILogger<LogEmailService> logger)
    {
        _logger = logger;
    }

    public Task EnviarAsync(string destinatario, string asunto, string cuerpo)
    {
        _logger.LogInformation(
            "[EMAIL SIMULADO] Para: {Destinatario} | Asunto: {Asunto}\n{Cuerpo}",
            destinatario, asunto, cuerpo);

        return Task.CompletedTask;
    }
}
