using ExchangePlatform.Domain.Enums;

namespace ExchangePlatform.Domain.Interfaces.Services;

public interface INotificacionService
{
    Task CrearAsync(Guid usuarioId, TipoNotificacion tipo,
                    string titulo, string mensaje,
                    string? entidadTipo = null, Guid? entidadId = null);
}