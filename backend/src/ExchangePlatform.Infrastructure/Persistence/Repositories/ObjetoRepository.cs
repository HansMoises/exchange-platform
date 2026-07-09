using ExchangePlatform.Domain.Entities;
using ExchangePlatform.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ExchangePlatform.Infrastructure.Persistence.Repositories;

public class ObjetoRepository : GenericRepository<Objeto>, IObjetoRepository
{
    public ObjetoRepository(ExchangePlatformDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Objeto>> GetDisponiblesAsync(
        string? search, int? categoriaId,
        int? departamentoId, int? provinciaId, int? distritoId,
        string? sortBy, string? sortOrder,
        int pageNumber, int pageSize)
    {
        var query = _dbSet
            .Include(o => o.Imagenes)
            .Where(o => o.Estado == Domain.Enums.EstadoObjeto.Disponible)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(o => o.Titulo.Contains(search) ||
                                     o.Descripcion.Contains(search));

        if (categoriaId.HasValue)
            query = query.Where(o => o.CategoriaId == categoriaId);

        if (departamentoId.HasValue)
            query = query.Where(o => o.DepartamentoId == departamentoId);

        if (provinciaId.HasValue)
            query = query.Where(o => o.ProvinciaId == provinciaId);

        if (distritoId.HasValue)
            query = query.Where(o => o.DistritoId == distritoId);

        query = AplicarOrden(query, sortBy, sortOrder);

        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    // RN-014: solo objetos Disponible son visibles en busquedas (no es un
    // filtro opcional, es una regla fija). API.md SS1.4: sortBy admite
    // createdAt (default), titulo, calificacionPromedio; sortOrder admite
    // asc/desc (default desc).
    private IQueryable<Objeto> AplicarOrden(
        IQueryable<Objeto> query, string? sortBy, string? sortOrder)
    {
        var ascendente = string.Equals(sortOrder, "asc", StringComparison.OrdinalIgnoreCase);

        if (string.Equals(sortBy, "titulo", StringComparison.OrdinalIgnoreCase))
            return ascendente
                ? query.OrderBy(o => o.Titulo)
                : query.OrderByDescending(o => o.Titulo);

        if (string.Equals(sortBy, "calificacionPromedio", StringComparison.OrdinalIgnoreCase))
        {
            var conCalificacion = query.Select(o => new
            {
                Objeto = o,
                Calificacion = _context.Usuarios
                    .Where(u => u.Id == o.UsuarioId)
                    .Select(u => u.CalificacionPromedio)
                    .FirstOrDefault()
            });

            return (ascendente
                ? conCalificacion.OrderBy(x => x.Calificacion)
                : conCalificacion.OrderByDescending(x => x.Calificacion))
                .Select(x => x.Objeto);
        }

        return ascendente
            ? query.OrderBy(o => o.CreatedAt)
            : query.OrderByDescending(o => o.CreatedAt);
    }

    public async Task<int> CountDisponiblesAsync(
        string? search, int? categoriaId,
        int? departamentoId, int? provinciaId, int? distritoId)
    {
        var query = _dbSet
            .Where(o => o.Estado == Domain.Enums.EstadoObjeto.Disponible)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(o => o.Titulo.Contains(search) ||
                                     o.Descripcion.Contains(search));

        if (categoriaId.HasValue)
            query = query.Where(o => o.CategoriaId == categoriaId);

        if (departamentoId.HasValue)
            query = query.Where(o => o.DepartamentoId == departamentoId);

        if (provinciaId.HasValue)
            query = query.Where(o => o.ProvinciaId == provinciaId);

        if (distritoId.HasValue)
            query = query.Where(o => o.DistritoId == distritoId);

        return await query.CountAsync();
    }

    public async Task<IReadOnlyList<Objeto>> GetByUsuarioIdAsync(Guid usuarioId) =>
        await _dbSet
            .Include(o => o.Imagenes)
            .Where(o => o.UsuarioId == usuarioId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

    public async Task<IReadOnlyList<Objeto>> GetDisponiblesByUsuarioIdAsync(Guid usuarioId) =>
        await _dbSet
            .Where(o => o.UsuarioId == usuarioId &&
                        o.Estado == Domain.Enums.EstadoObjeto.Disponible)
            .ToListAsync();

    public override async Task<Objeto?> GetByIdAsync(Guid id) =>
        await _dbSet
            .Include(o => o.Imagenes)
            .FirstOrDefaultAsync(o => o.Id == id);
}
