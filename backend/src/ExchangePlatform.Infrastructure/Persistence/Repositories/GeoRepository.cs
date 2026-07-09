using ExchangePlatform.Domain.Entities.Geo;
using ExchangePlatform.Domain.Entities.Maestras;
using Microsoft.EntityFrameworkCore;

namespace ExchangePlatform.Infrastructure.Persistence.Repositories;

public class GeoRepository
{
    private readonly ExchangePlatformDbContext _context;

    public GeoRepository(ExchangePlatformDbContext context)
    {
        _context = context;
    }

    public async Task<List<Departamento>> GetDepartamentosAsync() =>
        await _context.Departamentos
            .OrderBy(d => d.Nombre)
            .ToListAsync();

    public async Task<List<Provincia>> GetProvinciasByDepartamentoAsync(int departamentoId) =>
        await _context.Provincias
            .Where(p => p.DepartamentoId == departamentoId)
            .OrderBy(p => p.Nombre)
            .ToListAsync();

    public async Task<List<Distrito>> GetDistritosByProvinciaAsync(int provinciaId) =>
        await _context.Distritos
            .Where(d => d.ProvinciaId == provinciaId)
            .OrderBy(d => d.Nombre)
            .ToListAsync();

    public async Task<List<Categoria>> GetCategoriasActivasAsync() =>
        await _context.Categorias
            .Where(c => c.IsActive)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
}