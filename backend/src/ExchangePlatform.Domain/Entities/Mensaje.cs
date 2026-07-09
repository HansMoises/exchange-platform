using ExchangePlatform.Domain.Common;
using ExchangePlatform.Domain.Exceptions;

namespace ExchangePlatform.Domain.Entities;

public class Mensaje : BaseEntity
{
    public Guid IntercambioId { get; private set; }
    public Guid RemitenteId { get; private set; }
    public string Contenido { get; private set; } = string.Empty;
    public DateTime EnviadoEn { get; private set; } = DateTime.UtcNow;
    public bool IsLeido { get; private set; }

    // EF Core
    protected Mensaje() { }

    public Mensaje(Guid intercambioId, Guid remitenteId, string contenido)
    {
        if (string.IsNullOrWhiteSpace(contenido))
            throw new DomainException("El mensaje no puede estar vacío.");

        if (contenido.Length > 1000)
            throw new DomainException("El mensaje no puede superar 1000 caracteres.");

        IntercambioId = intercambioId;
        RemitenteId = remitenteId;
        Contenido = contenido;
    }

    public void MarcarLeido()
    {
        IsLeido = true;
        UpdatedAt = DateTime.UtcNow;
    }
}