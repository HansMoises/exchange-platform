using ExchangePlatform.Domain.Entities;

namespace ExchangePlatform.Domain.Interfaces.Repositories;

public interface IIntercambioRepository : IGenericRepository<Intercambio>
{
    Task<bool> TieneSolicitudActivaAsync(Guid solicitanteId, Guid objetoSolicitadoId);
    Task<IReadOnlyList<Intercambio>> GetByUsuarioIdAsync(
        Guid usuarioId, int pageNumber, int pageSize);
}