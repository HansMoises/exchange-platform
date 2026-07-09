using ExchangePlatform.Domain.Entities;
using ExchangePlatform.Domain.Enums;
using ExchangePlatform.Domain.Exceptions;
using FluentAssertions;

namespace ExchangePlatform.UnitTests.Domain;

// Maquina de estados de Intercambio: Pendiente -> Aceptado ->
// PendienteConfirmacion -> Completado (o Rechazado / Cancelado).
public class IntercambioTests
{
    private static Intercambio CrearIntercambioValido(Guid? solicitante = null, Guid? propietario = null) =>
        new(Guid.NewGuid(), Guid.NewGuid(), solicitante ?? Guid.NewGuid(), propietario ?? Guid.NewGuid());

    [Fact]
    public void Constructor_lanza_si_solicitante_y_propietario_son_el_mismo()
    {
        var usuarioId = Guid.NewGuid();
        var accion = () => new Intercambio(Guid.NewGuid(), Guid.NewGuid(), usuarioId, usuarioId);
        accion.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_lanza_si_los_objetos_son_el_mismo()
    {
        var objetoId = Guid.NewGuid();
        var accion = () => new Intercambio(objetoId, objetoId, Guid.NewGuid(), Guid.NewGuid());
        accion.Should().Throw<DomainException>();
    }

    [Fact]
    public void Aceptar_cambia_estado_a_Aceptado_y_registra_fecha()
    {
        var intercambio = CrearIntercambioValido();
        intercambio.Aceptar();

        intercambio.Estado.Should().Be(EstadoIntercambio.Aceptado);
        intercambio.FechaAceptacion.Should().NotBeNull();
    }

    [Fact]
    public void Aceptar_lanza_si_no_esta_pendiente()
    {
        var intercambio = CrearIntercambioValido();
        intercambio.Aceptar();

        var accion = () => intercambio.Aceptar();

        accion.Should().Throw<DomainException>();
    }

    [Fact]
    public void Rechazar_lanza_si_no_esta_pendiente()
    {
        var intercambio = CrearIntercambioValido();
        intercambio.Rechazar();

        var accion = () => intercambio.Rechazar();

        accion.Should().Throw<DomainException>();
    }

    [Fact]
    public void Confirmar_lanza_si_el_intercambio_no_esta_aceptado()
    {
        var intercambio = CrearIntercambioValido();

        var accion = () => intercambio.Confirmar(Guid.NewGuid());

        accion.Should().Throw<DomainException>();
    }

    [Fact]
    public void Confirmar_lanza_si_el_usuario_no_es_parte_del_intercambio()
    {
        var intercambio = CrearIntercambioValido();
        intercambio.Aceptar();

        var accion = () => intercambio.Confirmar(Guid.NewGuid());

        accion.Should().Throw<DomainException>();
    }

    [Fact]
    public void Confirmar_con_una_sola_parte_deja_PendienteConfirmacion()
    {
        var solicitanteId = Guid.NewGuid();
        var propietarioId = Guid.NewGuid();
        var intercambio = CrearIntercambioValido(solicitanteId, propietarioId);
        intercambio.Aceptar();

        intercambio.Confirmar(solicitanteId);

        intercambio.Estado.Should().Be(EstadoIntercambio.PendienteConfirmacion);
        intercambio.ConfirmacionSolicitante.Should().BeTrue();
        intercambio.ConfirmacionPropietario.Should().BeFalse();
    }

    [Fact]
    public void Confirmar_con_ambas_partes_completa_el_intercambio()
    {
        var solicitanteId = Guid.NewGuid();
        var propietarioId = Guid.NewGuid();
        var intercambio = CrearIntercambioValido(solicitanteId, propietarioId);
        intercambio.Aceptar();

        intercambio.Confirmar(solicitanteId);
        intercambio.Confirmar(propietarioId);

        intercambio.Estado.Should().Be(EstadoIntercambio.Completado);
        intercambio.FechaCompletado.Should().NotBeNull();
    }

    [Fact]
    public void Cancelar_lanza_si_ya_esta_Completado()
    {
        var solicitanteId = Guid.NewGuid();
        var propietarioId = Guid.NewGuid();
        var intercambio = CrearIntercambioValido(solicitanteId, propietarioId);
        intercambio.Aceptar();
        intercambio.Confirmar(solicitanteId);
        intercambio.Confirmar(propietarioId);

        var accion = () => intercambio.Cancelar();

        accion.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancelar_lanza_si_ya_esta_Rechazado()
    {
        var intercambio = CrearIntercambioValido();
        intercambio.Rechazar();

        var accion = () => intercambio.Cancelar();

        accion.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancelar_desde_Pendiente_es_valido()
    {
        var intercambio = CrearIntercambioValido();
        intercambio.Cancelar();

        intercambio.Estado.Should().Be(EstadoIntercambio.Cancelado);
    }
}
