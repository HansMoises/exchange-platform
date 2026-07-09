using ExchangePlatform.Domain.Entities;

namespace ExchangePlatform.Domain.Interfaces.Repositories;

public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task RevocarTodosDelUsuarioAsync(Guid usuarioId);
}