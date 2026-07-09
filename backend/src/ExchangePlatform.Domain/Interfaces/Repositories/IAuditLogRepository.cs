using ExchangePlatform.Domain.Common;

namespace ExchangePlatform.Domain.Interfaces.Repositories;

public interface IAuditLogRepository
{
    Task RegistrarAsync(Guid? usuarioId, string accion,
        string entidadTipo, string? entidadId,
        string resultado, string ipAddress, string? detalle = null);
}