using ExchangePlatform.Domain.Entities;
using ExchangePlatform.Domain.Enums;
using ExchangePlatform.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ExchangePlatform.Infrastructure.Persistence.Repositories;

public class ReporteRepository
    : GenericRepository<Reporte>, IReporteRepository
{
    public ReporteRepository(ExchangePlatformDbContext context)
        : base(context) { }

    public async Task<IReadOnlyList<Reporte>> GetPendientesAsync() =>
        await _dbSet
            .Where(r => r.EstadoReporte == EstadoReporte.Pendiente)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();

    public async Task<bool> ExisteReporteActivoAsync(
        Guid reportanteId, Guid entidadId) =>
        await _dbSet.AnyAsync(r =>
            r.ReportanteId == reportanteId &&
            r.EntidadId == entidadId &&
            r.EstadoReporte == EstadoReporte.Pendiente);
}