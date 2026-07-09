using ExchangePlatform.Domain.Entities;
using ExchangePlatform.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ExchangePlatform.Infrastructure.Persistence.Repositories;

public class CalificacionRepository
    : GenericRepository<Calificacion>, ICalificacionRepository
{
    public CalificacionRepository(ExchangePlatformDbContext context)
        : base(context) { }

    public async Task<IReadOnlyList<Calificacion>> GetByCalificadoIdAsync(
        Guid calificadoId) =>
        await _dbSet
            .Where(c => c.CalificadoId == calificadoId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

    public async Task<IReadOnlyList<Calificacion>> GetByIntercambioIdAsync(
        Guid intercambioId) =>
        await _dbSet
            .Where(c => c.IntercambioId == intercambioId)
            .ToListAsync();
}