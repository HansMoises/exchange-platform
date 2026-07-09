using ExchangePlatform.Domain.Entities;
using FluentAssertions;

namespace ExchangePlatform.UnitTests.Domain;

public class UsuarioTests
{
    private static Usuario CrearUsuarioValido() =>
        new("Nombres", "Apellidos", "test@example.com", "hash", "987654321", 1, 1, 1);

    [Fact]
    public void Usuario_nuevo_no_esta_bloqueado()
    {
        var usuario = CrearUsuarioValido();
        usuario.EstaBloqueado().Should().BeFalse();
    }

    [Fact]
    public void No_se_bloquea_antes_del_quinto_intento_fallido()
    {
        var usuario = CrearUsuarioValido();

        for (var i = 0; i < 4; i++)
        {
            usuario.RegistrarIntentoFallido();
        }

        usuario.EstaBloqueado().Should().BeFalse();
    }

    [Fact]
    public void Se_bloquea_en_el_quinto_intento_fallido()
    {
        var usuario = CrearUsuarioValido();

        for (var i = 0; i < 5; i++)
        {
            usuario.RegistrarIntentoFallido();
        }

        usuario.EstaBloqueado().Should().BeTrue();
    }

    [Fact]
    public void ReiniciarIntentos_desbloquea_y_pone_en_cero_el_contador()
    {
        var usuario = CrearUsuarioValido();
        for (var i = 0; i < 5; i++)
        {
            usuario.RegistrarIntentoFallido();
        }

        usuario.ReiniciarIntentos();

        usuario.EstaBloqueado().Should().BeFalse();
        usuario.FailedAttempts.Should().Be(0);
    }

    [Fact]
    public void IncrementarIntercambios_suma_uno_al_contador()
    {
        var usuario = CrearUsuarioValido();

        usuario.IncrementarIntercambios();
        usuario.IncrementarIntercambios();

        usuario.TotalIntercambios.Should().Be(2);
    }

    [Fact]
    public void Activar_y_Desactivar_alternan_IsActive()
    {
        var usuario = CrearUsuarioValido();

        usuario.Desactivar();
        usuario.IsActive.Should().BeFalse();

        usuario.Activar();
        usuario.IsActive.Should().BeTrue();
    }

    [Fact]
    public void CambiarRol_actualiza_el_RolId()
    {
        var usuario = CrearUsuarioValido();

        usuario.CambiarRol(2);

        usuario.RolId.Should().Be(2);
    }

    [Fact]
    public void ActualizarReputacion_actualiza_la_calificacion_promedio()
    {
        var usuario = CrearUsuarioValido();

        usuario.ActualizarReputacion(4.5m);

        usuario.CalificacionPromedio.Should().Be(4.5m);
    }
}
