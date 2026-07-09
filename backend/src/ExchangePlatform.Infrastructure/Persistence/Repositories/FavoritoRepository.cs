using ExchangePlatform.Domain.Entities;
using ExchangePlatform.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ExchangePlatform.Infrastructure.Persistence.Repositories;

public class FavoritoRepository
    : GenericRepository<Favorito>, IFavoritoRepository
{
    public FavoritoRepository(ExchangePlatformDbContext context)
        : base(context) { }

    public async Task<IReadOnlyList<Favorito>> GetByUsuarioIdAsync(Guid usuarioId) =>
        await _dbSet
            .Where(f => f.UsuarioId == usuarioId)
            .OrderByDescending(f => f.AgregadoEn)
            .ToListAsync();

    public async Task<Favorito?> GetByUsuarioYObjetoAsync(
        Guid usuarioId, Guid objetoId) =>
        await _dbSet
            .FirstOrDefaultAsync(f =>
                f.UsuarioId == usuarioId && f.ObjetoId == objetoId);
}