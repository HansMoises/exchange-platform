using ExchangePlatform.Domain.Entities;

namespace ExchangePlatform.Domain.Interfaces.Repositories;

public interface IFavoritoRepository : IGenericRepository<Favorito>
{
    Task<IReadOnlyList<Favorito>> GetByUsuarioIdAsync(Guid usuarioId);
    Task<Favorito?> GetByUsuarioYObjetoAsync(Guid usuarioId, Guid objetoId);
}