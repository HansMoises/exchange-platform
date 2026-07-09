using ExchangePlatform.Domain.Entities;

namespace ExchangePlatform.Domain.Interfaces.Repositories;

public interface IObjetoRepository : IGenericRepository<Objeto>
{
    Task<IReadOnlyList<Objeto>> GetDisponiblesAsync(
        string? search, int? categoriaId,
        int? departamentoId, int? provinciaId, int? distritoId,
        string? sortBy, string? sortOrder,
        int pageNumber, int pageSize);
    Task<int> CountDisponiblesAsync(
        string? search, int? categoriaId,
        int? departamentoId, int? provinciaId, int? distritoId);
    Task<IReadOnlyList<Objeto>> GetByUsuarioIdAsync(Guid usuarioId);
    Task<IReadOnlyList<Objeto>> GetDisponiblesByUsuarioIdAsync(Guid usuarioId);
}