using ExchangePlatform.Domain.Entities.Maestras;
using ExchangePlatform.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ExchangePlatform.Infrastructure.Persistence.Repositories;

public class CategoriaRepository : ICategoriaRepository
{
    private readonly ExchangePlatformDbContext _context;

    public CategoriaRepository(ExchangePlatformDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Categoria>> GetActivasAsync() =>
        await _context.Categorias
            .Where(c => c.IsActive)
            .OrderBy(c => c.Nombre)
            .ToListAsync();

    public async Task<Categoria?> GetByIdAsync(int id) =>
        await _context.Categorias.FirstOrDefaultAsync(c => c.Id == id);
}
