using ExchangePlatform.Domain.Entities;

namespace ExchangePlatform.Domain.Interfaces.Repositories;

public interface IUsuarioRepository : IGenericRepository<Usuario>
{
    Task<Usuario?> GetByEmailAsync(string email);
    Task<bool> ExisteEmailAsync(string email);
}