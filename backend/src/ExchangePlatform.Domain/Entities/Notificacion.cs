using ExchangePlatform.Domain.Common;
using ExchangePlatform.Domain.Enums;

namespace ExchangePlatform.Domain.Entities;

public class Notificacion : BaseEntity
{
    public Guid UsuarioId { get; private set; }
    public TipoNotificacion Tipo { get; private set; }
    public string Titulo { get; private set; } = string.Empty;
    public string Mensaje { get; private set; } = string.Empty;
    public bool IsLeida { get; private set; }
    public string? EntidadTipo { get; private set; }
    public Guid? EntidadId { get; private set; }
    public DateTime CreadaEn { get; private set; } = DateTime.UtcNow;

    // EF Core
    protected Notificacion() { }

    public Notificacion(Guid usuarioId, TipoNotificacion tipo,
                        string titulo, string mensaje,
                        string? entidadTipo = null, Guid? entidadId = null)
    {
        UsuarioId = usuarioId;
        Tipo = tipo;
        Titulo = titulo;
        Mensaje = mensaje;
        EntidadTipo = entidadTipo;
        EntidadId = entidadId;
    }

    public void MarcarLeida()
    {
        IsLeida = true;
        UpdatedAt = DateTime.UtcNow;
    }
}