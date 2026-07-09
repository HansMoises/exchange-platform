using ExchangePlatform.Domain.Entities.Maestras;

namespace ExchangePlatform.Domain.Interfaces.Repositories;

public interface ICategoriaRepository
{
    Task<IReadOnlyList<Categoria>> GetActivasAsync();
    Task<Categoria?> GetByIdAsync(int id);
}
