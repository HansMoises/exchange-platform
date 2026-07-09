using ExchangePlatform.Domain.Entities;
using ExchangePlatform.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ExchangePlatform.Infrastructure.Persistence.Repositories;

public class NotificacionRepository
    : GenericRepository<Notificacion>, INotificacionRepository
{
    public NotificacionRepository(ExchangePlatformDbContext context)
        : base(context) { }

    public async Task<IReadOnlyList<Notificacion>> GetByUsuarioIdAsync(
        Guid usuarioId) =>
        await _dbSet
            .Where(n => n.UsuarioId == usuarioId)
            .OrderByDescending(n => n.CreadaEn)
            .ToListAsync();

    public async Task MarcarTodasLeidasAsync(Guid usuarioId)
    {
        var notificaciones = await _dbSet
            .Where(n => n.UsuarioId == usuarioId && !n.IsLeida)
            .ToListAsync();

        foreach (var n in notificaciones)
            n.MarcarLeida();
    }
}