using ExchangePlatform.Domain.Entities;
using ExchangePlatform.Domain.Exceptions;
using FluentAssertions;

namespace ExchangePlatform.UnitTests.Domain;

public class MensajeTests
{
    [Fact]
    public void Constructor_valido_asigna_propiedades()
    {
        var intercambioId = Guid.NewGuid();
        var remitenteId = Guid.NewGuid();

        var mensaje = new Mensaje(intercambioId, remitenteId, "Hola, coordinemos el intercambio.");

        mensaje.IntercambioId.Should().Be(intercambioId);
        mensaje.RemitenteId.Should().Be(remitenteId);
        mensaje.Contenido.Should().Be("Hola, coordinemos el intercambio.");
        mensaje.IsLeido.Should().BeFalse();
    }

    [Fact]
    public void Constructor_lanza_si_el_contenido_esta_vacio()
    {
        var accion = () => new Mensaje(Guid.NewGuid(), Guid.NewGuid(), "   ");

        accion.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_lanza_si_supera_1000_caracteres()
    {
        var largo = new string('a', 1001);

        var accion = () => new Mensaje(Guid.NewGuid(), Guid.NewGuid(), largo);

        accion.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarcarLeido_pone_IsLeido_en_true()
    {
        var mensaje = new Mensaje(Guid.NewGuid(), Guid.NewGuid(), "Contenido");

        mensaje.MarcarLeido();

        mensaje.IsLeido.Should().BeTrue();
    }
}
