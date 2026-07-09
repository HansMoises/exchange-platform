using ExchangePlatform.Domain.Common;
using ExchangePlatform.Domain.Entities;

namespace ExchangePlatform.Domain.Interfaces.Repositories;

public interface ICalificacionRepository : IGenericRepository<Calificacion>
{
    Task<IReadOnlyList<Calificacion>> GetByCalificadoIdAsync(Guid calificadoId);
    Task<IReadOnlyList<Calificacion>> GetByIntercambioIdAsync(Guid intercambioId);
}