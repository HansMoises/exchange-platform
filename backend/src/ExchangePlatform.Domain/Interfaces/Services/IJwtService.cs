using ExchangePlatform.Domain.Entities;

namespace ExchangePlatform.Domain.Interfaces.Services;

public interface IJwtService
{
    string GenerarAccessToken(Usuario usuario);
    string GenerarRefreshToken();
}