using ExchangePlatform.Domain.Entities;
using ExchangePlatform.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ExchangePlatform.Infrastructure.Persistence.Repositories;

public class PasswordResetTokenRepository : GenericRepository<PasswordResetToken>, IPasswordResetTokenRepository
{
    public PasswordResetTokenRepository(ExchangePlatformDbContext context) : base(context) { }

    public async Task<PasswordResetToken?> GetByTokenAsync(string token) =>
        await _dbSet.FirstOrDefaultAsync(t => t.Token == token);

    public async Task InvalidarTodosDelUsuarioAsync(Guid usuarioId)
    {
        var tokens = await _dbSet
            .Where(t => t.UsuarioId == usuarioId && !t.IsUsed)
            .ToListAsync();

        foreach (var t in tokens)
            t.MarcarUsado();
    }
}
