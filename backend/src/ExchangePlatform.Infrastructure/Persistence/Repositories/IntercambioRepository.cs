using ExchangePlatform.Domain.Entities;
using ExchangePlatform.Domain.Enums;
using ExchangePlatform.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ExchangePlatform.Infrastructure.Persistence.Repositories;

public class IntercambioRepository : GenericRepository<Intercambio>, IIntercambioRepository
{
    public IntercambioRepository(ExchangePlatformDbContext context) : base(context) { }

    public async Task<bool> TieneSolicitudActivaAsync(
        Guid solicitanteId, Guid objetoSolicitadoId) =>
        await _dbSet.AnyAsync(i =>
            i.UsuarioSolicitanteId == solicitanteId &&
            i.ObjetoSolicitadoId == objetoSolicitadoId &&
            i.Estado != EstadoIntercambio.Rechazado &&
            i.Estado != EstadoIntercambio.Cancelado &&
            i.Estado != EstadoIntercambio.Completado);

    public async Task<IReadOnlyList<Intercambio>> GetByUsuarioIdAsync(
        Guid usuarioId, int pageNumber, int pageSize) =>
        await _dbSet
            .Where(i => i.UsuarioSolicitanteId == usuarioId ||
                        i.UsuarioPropietarioId == usuarioId)
            .OrderByDescending(i => i.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
}