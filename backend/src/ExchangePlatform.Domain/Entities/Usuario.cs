using ExchangePlatform.Domain.Common;

namespace ExchangePlatform.Domain.Entities;

public class Usuario : BaseEntity
{
    public string Nombres { get; private set; } = string.Empty;
    public string Apellidos { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Telefono { get; private set; } = string.Empty;
    public string? FotoPerfil { get; private set; }
    public int RolId { get; private set; } = 3;
    public int DepartamentoId { get; private set; }
    public int ProvinciaId { get; private set; }
    public int DistritoId { get; private set; }
    public decimal? Latitud { get; private set; }
    public decimal? Longitud { get; private set; }
    public bool IsActive { get; private set; } = true;
    public decimal CalificacionPromedio { get; private set; } = 0;
    public int TotalIntercambios { get; private set; } = 0;
    public int FailedAttempts { get; private set; } = 0;
    public DateTime? LockedUntil { get; private set; }

    // EF Core
    protected Usuario() { }

    public Usuario(string nombres, string apellidos, string email,
                   string passwordHash, string telefono,
                   int departamentoId, int provinciaId, int distritoId)
    {
        Nombres = nombres;
        Apellidos = apellidos;
        Email = email;
        PasswordHash = passwordHash;
        Telefono = telefono;
        DepartamentoId = departamentoId;
        ProvinciaId = provinciaId;
        DistritoId = distritoId;
    }

    public void ActualizarPerfil(string nombres, string apellidos,
                                  string telefono, int departamentoId,
                                  int provinciaId, int distritoId)
    {
        Nombres = nombres;
        Apellidos = apellidos;
        Telefono = telefono;
        DepartamentoId = departamentoId;
        ProvinciaId = provinciaId;
        DistritoId = distritoId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ActualizarFoto(string url)
    {
        FotoPerfil = url;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ActualizarPassword(string nuevoHash)
    {
        PasswordHash = nuevoHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RegistrarIntentoFallido()
    {
        FailedAttempts++;
        if (FailedAttempts >= 5)
            LockedUntil = DateTime.UtcNow.AddMinutes(15);
    }

    public void ReiniciarIntentos()
    {
        FailedAttempts = 0;
        LockedUntil = null;
    }

    public bool EstaBloqueado() =>
        LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow;

    public void ActualizarReputacion(decimal promedio)
    {
        CalificacionPromedio = promedio;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementarIntercambios()
    {
        TotalIntercambios++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Desactivar()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activar()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void CambiarRol(int nuevoRolId)
    {
        RolId = nuevoRolId;
        UpdatedAt = DateTime.UtcNow;
    }
}