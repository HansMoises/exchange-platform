using ExchangePlatform.Domain.Entities;
using ExchangePlatform.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ExchangePlatform.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(ExchangePlatformDbContext context) : base(context) { }

    public async Task<RefreshToken?> GetByTokenAsync(string token) =>
        await _dbSet.FirstOrDefaultAsync(r => r.Token == token);

    public async Task RevocarTodosDelUsuarioAsync(Guid usuarioId)
    {
        var tokens = await _dbSet
            .Where(r => r.UsuarioId == usuarioId && !r.IsRevoked)
            .ToListAsync();

        foreach (var t in tokens)
            t.Revocar();
    }
}