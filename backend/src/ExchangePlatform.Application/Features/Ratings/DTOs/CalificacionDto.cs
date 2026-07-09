namespace ExchangePlatform.Application.Features.Ratings.DTOs;

public class CalificacionDto
{
    public Guid Id { get; set; }
    public Guid IntercambioId { get; set; }
    public Guid CalificadorId { get; set; }
    public string CalificadorNombres { get; set; } = string.Empty;
    public Guid CalificadoId { get; set; }
    public int Puntuacion { get; set; }
    public string? Comentario { get; set; }
    public DateTime CreadoEn { get; set; }
}