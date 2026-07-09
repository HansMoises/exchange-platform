using ExchangePlatform.Domain.Common;

namespace ExchangePlatform.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public string Token { get; private set; } = string.Empty;
    public Guid UsuarioId { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public string CreatedByIp { get; private set; } = string.Empty;

    // EF Core
    protected RefreshToken() { }

    public RefreshToken(string token, Guid usuarioId,
                        DateTime expiresAt, string createdByIp)
    {
        Token = token;
        UsuarioId = usuarioId;
        ExpiresAt = expiresAt;
        CreatedByIp = createdByIp;
    }

    public bool EstaVigente() =>
        !IsRevoked && DateTime.UtcNow < ExpiresAt;

    public void Revocar()
    {
        IsRevoked = true;
        UpdatedAt = DateTime.UtcNow;
    }
}