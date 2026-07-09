using ExchangePlatform.Domain.Entities;

namespace ExchangePlatform.Domain.Interfaces.Repositories;

public interface INotificacionRepository : IGenericRepository<Notificacion>
{
    Task<IReadOnlyList<Notificacion>> GetByUsuarioIdAsync(Guid usuarioId);
    Task MarcarTodasLeidasAsync(Guid usuarioId);
}