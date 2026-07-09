namespace ExchangePlatform.Application.Features.Auth.DTOs;

public class UsuarioDto
{
    public Guid Id { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string? FotoPerfil { get; set; }
    public decimal CalificacionPromedio { get; set; }
    public int TotalIntercambios { get; set; }
}