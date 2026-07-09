namespace ExchangePlatform.Application.Features.Exchanges.DTOs;

public class IntercambioDto
{
    public Guid Id { get; set; }
    public Guid ObjetoSolicitadoId { get; set; }
    public string ObjetoSolicitadoTitulo { get; set; } = string.Empty;
    public Guid ObjetoOfrecidoId { get; set; }
    public string ObjetoOfrecidoTitulo { get; set; } = string.Empty;
    public Guid UsuarioSolicitanteId { get; set; }
    public string UsuarioSolicitanteNombres { get; set; } = string.Empty;
    public Guid UsuarioPropietarioId { get; set; }
    public string UsuarioPropietarioNombres { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string? MensajeInicial { get; set; }
    public bool ConfirmacionSolicitante { get; set; }
    public bool ConfirmacionPropietario { get; set; }
    public DateTime? FechaAceptacion { get; set; }
    public DateTime? FechaCompletado { get; set; }
    public DateTime CreadoEn { get; set; }
}