using ExchangePlatform.Domain.Entities;

namespace ExchangePlatform.Domain.Interfaces.Repositories;

public interface IReporteRepository : IGenericRepository<Reporte>
{
    Task<IReadOnlyList<Reporte>> GetPendientesAsync();
    Task<bool> ExisteReporteActivoAsync(Guid reportanteId, Guid entidadId);
}