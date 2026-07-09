namespace ExchangePlatform.Application.Features.Users.DTOs;

public class PerfilUsuarioDto
{
    public Guid Id { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? FotoPerfil { get; set; }
    public int RolId { get; set; }
    public int DepartamentoId { get; set; }
    public int ProvinciaId { get; set; }
    public int DistritoId { get; set; }
    public decimal CalificacionPromedio { get; set; }
    public int TotalIntercambios { get; set; }
    public DateTime MiembroDesde { get; set; }
}