using ExchangePlatform.Domain.Common;

namespace ExchangePlatform.Domain.Entities;

public class PasswordResetToken : BaseEntity
{
    public string Token { get; private set; } = string.Empty;
    public Guid UsuarioId { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; }

    protected PasswordResetToken() { }

    public PasswordResetToken(string token, Guid usuarioId, DateTime expiresAt)
    {
        Token = token;
        UsuarioId = usuarioId;
        ExpiresAt = expiresAt;
    }

    public bool EstaVigente() =>
        !IsUsed && DateTime.UtcNow < ExpiresAt;

    public void MarcarUsado()
    {
        IsUsed = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
