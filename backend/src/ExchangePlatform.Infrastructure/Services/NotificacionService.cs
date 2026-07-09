using ExchangePlatform.Domain.Entities;
using ExchangePlatform.Domain.Enums;
using ExchangePlatform.Domain.Interfaces.Services;
using ExchangePlatform.Infrastructure.Persistence;

namespace ExchangePlatform.Infrastructure.Services;

public class NotificacionService : INotificacionService
{
    private readonly ExchangePlatformDbContext _context;

    public NotificacionService(ExchangePlatformDbContext context)
    {
        _context = context;
    }

    public async Task CrearAsync(Guid usuarioId, TipoNotificacion tipo,
        string titulo, string mensaje,
        string? entidadTipo = null, Guid? entidadId = null)
    {
        var notificacion = new Notificacion(
            usuarioId, tipo, titulo, mensaje, entidadTipo, entidadId);

        notificacion.CreatedBy = usuarioId;
        await _context.Notificaciones.AddAsync(notificacion);
        await _context.SaveChangesAsync();
    }
}