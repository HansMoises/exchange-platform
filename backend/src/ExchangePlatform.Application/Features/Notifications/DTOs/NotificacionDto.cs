namespace ExchangePlatform.Application.Features.Notifications.DTOs;

public class NotificacionDto
{
    public Guid Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public bool IsLeida { get; set; }
    public string? EntidadTipo { get; set; }
    public Guid? EntidadId { get; set; }
    public DateTime CreadaEn { get; set; }
}