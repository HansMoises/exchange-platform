using ExchangePlatform.Domain.Entities;
using ExchangePlatform.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ExchangePlatform.Infrastructure.Persistence.Repositories;

public class UsuarioRepository : GenericRepository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(ExchangePlatformDbContext context) : base(context) { }

    public async Task<Usuario?> GetByEmailAsync(string email) =>
        await _dbSet.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<bool> ExisteEmailAsync(string email) =>
        await _dbSet.AnyAsync(u => u.Email == email);
}