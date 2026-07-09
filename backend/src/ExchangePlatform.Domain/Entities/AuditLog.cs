namespace ExchangePlatform.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid? UsuarioId { get; private set; }
    public string Accion { get; private set; } = string.Empty;
    public string EntidadTipo { get; private set; } = string.Empty;
    public string? EntidadId { get; private set; }
    public string? Detalle { get; private set; }
    public string Resultado { get; private set; } = string.Empty;
    public string IpAddress { get; private set; } = string.Empty;
    public DateTime OcurridoEn { get; private set; } = DateTime.UtcNow;

    protected AuditLog() { }

    public AuditLog(Guid? usuarioId, string accion, string entidadTipo,
        string? entidadId, string resultado, string ipAddress,
        string? detalle = null)
    {
        UsuarioId = usuarioId;
        Accion = accion;
        EntidadTipo = entidadTipo;
        EntidadId = entidadId;
        Resultado = resultado;
        IpAddress = ipAddress;
        Detalle = detalle;
    }
}