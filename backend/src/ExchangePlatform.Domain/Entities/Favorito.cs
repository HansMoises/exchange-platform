using ExchangePlatform.Domain.Common;

namespace ExchangePlatform.Domain.Entities;

public class Favorito : BaseEntity
{
    public Guid UsuarioId { get; private set; }
    public Guid ObjetoId { get; private set; }
    public DateTime AgregadoEn { get; private set; } = DateTime.UtcNow;

    // EF Core
    protected Favorito() { }

    public Favorito(Guid usuarioId, Guid objetoId)
    {
        UsuarioId = usuarioId;
        ObjetoId = objetoId;
    }
}